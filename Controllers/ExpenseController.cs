namespace PetrolStationMS.Controllers;

public class ExpenseController : BaseController
{
    public static readonly string[] Categories = { "Salaries", "Utilities", "Maintenance", "Supplies", "Transport", "Taxes & Fees", "Other" };

    public Result<List<Expense>> GetRange(DateTime from, DateTime to) => Run(() =>
    {
        Need("Expenses");
        return new ExpenseRepository().GetRange(from, to);
    });

    public Result Add(Expense e) => Run(() =>
    {
        Need("Expenses");
        e.Payee = Clean(e.Payee); e.Description = Clean(e.Description);
        Require(Categories.Contains(e.Category), "Select a category.");
        Require(e.Amount > 0, "Amount must be greater than 0.");
        Require(e.Payee != "", "Payee is required.");
        Require(e.ExpenseDate.Date <= DateTime.Today, "Expense date cannot be in the future.");
        e.RecordedBy = Session.UserId;
        var id = new ExpenseRepository().Insert(e);
        AuditController.Write("EXPENSE", $"Expense #{id}: {e.Category} {e.Amount:N2} to {e.Payee}");
        return Result.Success();
    });
}
