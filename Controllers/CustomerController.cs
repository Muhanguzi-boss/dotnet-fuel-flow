namespace PetrolStationMS.Controllers;

public class CustomerController : BaseController
{
    readonly CustomerRepository _repo = new();
    public Result<List<Customer>> GetAll(bool activeOnly = false) => Run(() => _repo.GetAll(activeOnly));

    public Result Save(Customer x) => Run(() =>
    {
        Need("Customers");
        x.Name = Clean(x.Name);
        Require(x.Name != "", "Customer name is required.");
        Require(x.CreditLimit >= 0, "Credit limit cannot be negative.");
        if (x.Id == 0) { _repo.Insert(x); AuditController.Write("CUSTOMER_ADD", $"Added customer {x.Name}"); }
        else
        {
            _repo.Update(x);
            AuditController.Write("CUSTOMER_EDIT", $"Edited customer {x.Name} (limit {x.CreditLimit:N2})");
        }
        return Result.Success();
    });

    public Result SetActive(Customer x, bool active) { x.IsActive = active; return Save(x); }
}
