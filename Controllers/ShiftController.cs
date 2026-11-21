namespace PetrolStationMS.Controllers;

public class ShiftController : BaseController
{
    public Result<Shift?> GetOpen() => Run(() => new ShiftRepository().GetOpen());
    public Result<List<Shift>> GetRecent() => Run(() => new ShiftRepository().GetRecent());
    public Result<List<PumpReading>> GetReadings(int shiftId) => Run(() => new ShiftRepository().GetReadings(shiftId));

    // Active pumps with the last closing meter as the suggested opening reading
    public Result<List<PumpReading>> PrepareOpening() => Run(() =>
    {
        var repo = new ShiftRepository();
        return new PumpRepository().GetAll(true).Select(p => new PumpReading
        {
            PumpId = p.Id, PumpCode = p.Code, ProductName = p.ProductName, OpeningReading = repo.LastClosing(p.Id)
        }).ToList();
    });

    public Result Open(List<PumpReading> readings) => Run(() =>
    {
        Need("Shifts");
        Require(readings.Count > 0, "There are no active pumps to open a shift for.");
        foreach (var r in readings) Require(r.OpeningReading >= 0, $"Opening reading for {r.PumpCode} cannot be negative.");
        Db.InTransaction((c, t) =>
        {
            var shifts = new ShiftRepository(c, t);
            if (shifts.GetOpen() is Shift open) throw new BusinessException($"Shift #{open.Id} is still open. Close it before opening a new one.");
            foreach (var r in readings)
            {
                var last = shifts.LastClosing(r.PumpId);
                if (r.OpeningReading < last)
                    throw new BusinessException($"Opening reading for {r.PumpCode} ({r.OpeningReading:N2}) is lower than the last closing reading ({last:N2}).");
            }
            var id = shifts.Insert(Session.UserId);
            foreach (var r in readings) shifts.InsertReading(id, r.PumpId, r.OpeningReading);
            AuditController.Write("SHIFT_OPEN", $"Shift #{id} opened with {readings.Count} pumps", c, t);
            return 0;
        });
        return Result.Success();
    });

    // Close: saves closing readings, creates sales and lowers tank stock in ONE transaction
    public Result Close(int shiftId, List<PumpReading> readings) => Run(() =>
    {
        Need("Shifts");
        foreach (var r in readings)
        {
            Require(r.ClosingReading.HasValue, $"Enter the closing reading for {r.PumpCode}.");
            Require(r.ClosingReading!.Value >= r.OpeningReading, $"Closing reading for {r.PumpCode} must be at least the opening reading ({r.OpeningReading:N2}).");
        }
        Db.InTransaction((c, t) =>
        {
            var shifts = new ShiftRepository(c, t);
            var shift = shifts.Get(shiftId, true) ?? throw new BusinessException("Shift not found.");
            Require(shift.Status == "Open", "This shift is already closed.");
            var sales = new SaleRepository(c, t);
            var tanks = new TankRepository(c, t);
            var pumps = new PumpRepository(c, t);
            var prices = new PriceRepository(c, t);
            var now = DateTime.Now;
            decimal totalValue = 0;
            foreach (var r in readings)
            {
                shifts.SetClosing(r.Id, r.ClosingReading!.Value);
                var litres = r.ClosingReading.Value - r.OpeningReading;
                if (litres <= 0) continue;
                var pump = pumps.Get(r.PumpId)!;
                var price = prices.GetPriceAt(pump.ProductId, now) ?? throw new BusinessException($"No price is set for {pump.ProductName}.");
                if (!tanks.AdjustStock(pump.TankId, -litres))
                    throw new BusinessException($"Not enough fuel in tank {pump.TankCode} for pump {pump.Code} ({litres:N2} L sold). Check the readings or tank stock.");
                sales.Insert(shiftId, pump.Id, pump.ProductId, litres, price);
                totalValue += Math.Round(litres * price, 2);
            }
            shifts.Close(shiftId);
            AuditController.Write("SHIFT_CLOSE", $"Shift #{shiftId} closed. Sales value {totalValue:N2}", c, t);
            return 0;
        });
        return Result.Success();
    });
}
