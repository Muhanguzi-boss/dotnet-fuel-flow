namespace PetrolStationMS.Controllers;

public class ProductController : BaseController
{
    readonly ProductRepository _repo = new();
    public Result<List<Product>> GetAll(bool activeOnly = false) => Run(() => _repo.GetAll(activeOnly));

    public Result Save(Product p) => Run(() =>
    {
        Need("Products");
        p.Code = Clean(p.Code).ToUpper(); p.Name = Clean(p.Name); p.Unit = Clean(p.Unit);
        Require(p.Code != "", "Product code is required.");
        Require(p.Name != "", "Product name is required.");
        Require(p.Unit != "", "Unit is required.");
        if (p.Id == 0) { _repo.Insert(p); AuditController.Write("PRODUCT_ADD", $"Added product {p.Code}"); }
        else { _repo.Update(p); AuditController.Write("PRODUCT_EDIT", $"Edited product {p.Code}"); }
        return Result.Success();
    });

    public Result SetActive(Product p, bool active) { p.IsActive = active; return Save(p); }
}
