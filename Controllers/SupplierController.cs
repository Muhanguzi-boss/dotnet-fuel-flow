namespace PetrolStationMS.Controllers;

public class SupplierController : BaseController
{
    readonly SupplierRepository _repo = new();
    public Result<List<Supplier>> GetAll(bool activeOnly = false) => Run(() => _repo.GetAll(activeOnly));

    public Result Save(Supplier s) => Run(() =>
    {
        Need("Suppliers");
        s.Name = Clean(s.Name);
        Require(s.Name != "", "Supplier name is required.");
        if (!string.IsNullOrWhiteSpace(s.Email)) Require(s.Email.Contains('@') && s.Email.Contains('.'), "Email address looks invalid.");
        if (s.Id == 0) { _repo.Insert(s); AuditController.Write("SUPPLIER_ADD", $"Added supplier {s.Name}"); }
        else { _repo.Update(s); AuditController.Write("SUPPLIER_EDIT", $"Edited supplier {s.Name}"); }
        return Result.Success();
    });

    public Result SetActive(Supplier s, bool active) { s.IsActive = active; return Save(s); }
}
