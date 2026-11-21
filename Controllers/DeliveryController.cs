namespace PetrolStationMS.Controllers;

public class DeliveryController : BaseController
{
    public Result<List<Delivery>> GetRange(DateTime from, DateTime to) => Run(() => new DeliveryRepository().GetRange(from, to));

    // Records a delivery and raises tank stock in ONE transaction
    public Result Record(Delivery d) => Run(() =>
    {
        Need("Deliveries");
        d.InvoiceRef = Clean(d.InvoiceRef);
        Require(d.SupplierId > 0, "Select a supplier.");
        Require(d.TankId > 0, "Select a tank.");
        Require(d.ProductId > 0, "Select a product.");
        Require(d.Quantity > 0, "Quantity must be greater than 0.");
        Require(d.UnitCost >= 0, "Unit cost cannot be negative.");
        Require(d.ReceivedAt <= DateTime.Now.AddMinutes(5), "Delivery date/time cannot be in the future.");
        d.ReceivedBy = Session.UserId;

        Db.InTransaction((c, t) =>
        {
            var tank = new TankRepository(c, t).Get(d.TankId, true) ?? throw new BusinessException("Tank not found.");
            Require(tank.IsActive, "That tank is inactive.");
            Require(tank.ProductId == d.ProductId, $"Product does not match the tank (tank {tank.Code} holds {tank.ProductName}).");
            if (tank.CurrentStock + d.Quantity > tank.Capacity)
                throw new BusinessException($"Delivery would exceed tank capacity. Free space in {tank.Code}: {tank.Capacity - tank.CurrentStock:N2} L.");
            var id = new DeliveryRepository(c, t).Insert(d);
            if (!new TankRepository(c, t).AdjustStock(d.TankId, d.Quantity)) throw new BusinessException("Could not update tank stock.");
            AuditController.Write("DELIVERY", $"Delivery #{id}: {d.Quantity:N2} L into {tank.Code}, invoice {d.InvoiceRef}", c, t);
            return 0;
        });
        return Result.Success();
    });

    // Void: Manager only; keeps the row and removes the litres from the tank
    public Result Void(int id, string reason) => Run(() =>
    {
        Need("Voids");
        reason = Clean(reason);
        Require(reason.Length >= 3, "A void reason is required.");
        Db.InTransaction((c, t) =>
        {
            var d = new DeliveryRepository(c, t).Get(id, true) ?? throw new BusinessException("Delivery not found.");
            Require(!d.IsVoid, "This delivery is already void.");
            if (!new TankRepository(c, t).AdjustStock(d.TankId, -d.Quantity))
                throw new BusinessException("Cannot void: the tank no longer holds enough fuel to remove this delivery.");
            new DeliveryRepository(c, t).MarkVoid(id, reason, Session.UserId);
            AuditController.Write("VOID_DELIVERY", $"Voided delivery #{id} ({d.Quantity:N2} L). Reason: {reason}", c, t);
            return 0;
        });
        return Result.Success();
    });
}
