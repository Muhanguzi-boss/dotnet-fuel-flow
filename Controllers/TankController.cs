namespace PetrolStationMS.Controllers;

public class TankController : BaseController
{
    readonly TankRepository _repo = new();
    public Result<List<Tank>> GetAll(bool activeOnly = false) => Run(() => _repo.GetAll(activeOnly));

    public Result Save(Tank k) => Run(() =>
    {
        Need("Tanks");
        k.Code = Clean(k.Code).ToUpper();
        Require(k.Code != "", "Tank code is required.");
        Require(k.ProductId > 0, "Select a product.");
        Require(k.Capacity > 0, "Capacity must be greater than 0.");
        Require(k.CurrentStock >= 0 && k.CurrentStock <= k.Capacity, "Current stock must be between 0 and the capacity.");
        Require(k.LowThreshold >= 0 && k.LowThreshold <= k.Capacity, "Low-stock threshold must be between 0 and the capacity.");
        if (k.Id == 0) { _repo.Insert(k); AuditController.Write("TANK_ADD", $"Added tank {k.Code}"); }
        else
        {
            var old = _repo.Get(k.Id);
            _repo.Update(k);
            AuditController.Write("TANK_EDIT", $"Edited tank {k.Code}" + (old != null && old.CurrentStock != k.CurrentStock ? $" (stock {old.CurrentStock} -> {k.CurrentStock})" : ""));
        }
        return Result.Success();
    });

    public Result SetActive(Tank k, bool active) { k.IsActive = active; return Save(k); }
}
