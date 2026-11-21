namespace PetrolStationMS.Controllers;

public class SaleController : BaseController
{
    public Result<List<Sale>> GetRange(DateTime from, DateTime to) => Run(() => { Need("Sales"); return new SaleRepository().GetRange(from, to); });

    public Result Void(int id, string reason) => Run(() =>
    {
        Need("Voids");
        reason = Clean(reason);
        Require(reason.Length >= 3, "A void reason is required.");
        Db.InTransaction((c, t) =>
        {
            var repo = new SaleRepository(c, t);
            var s = repo.Get(id, true) ?? throw new BusinessException("Sale not found.");
            Require(!s.IsVoid, "This sale is already void.");
            Require(repo.CountActivePayments(id) == 0, "Void the payments linked to this sale first.");
            var tankId = repo.TankIdOfPump(s.PumpId);
            if (!new TankRepository(c, t).AdjustStock(tankId, s.Litres))
                throw new BusinessException("Cannot restore stock: it would exceed the tank capacity.");
            repo.MarkVoid(id, reason, Session.UserId);
            AuditController.Write("VOID_SALE", $"Voided sale #{id} ({s.Litres:N2} L, {s.Total:N2}). Reason: {reason}", c, t);
            return 0;
        });
        return Result.Success();
    });
}
