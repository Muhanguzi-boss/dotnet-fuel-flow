namespace PetrolStationMS.Controllers;

public class ReportController : BaseController
{
    // key -> (title, needs date range)
    public static readonly (string Key, string Title, bool UsesDates)[] Reports =
    {
        ("SalesProduct", "Sales summary by product", true),
        ("SalesPump", "Sales summary by pump", true),
        ("SalesAttendant", "Sales summary by attendant", true),
        ("Stock", "Stock levels", false),
        ("Deliveries", "Deliveries by supplier", true),
        ("Shifts", "Shift reconciliation", true),
        ("Expenses", "Expenses by category", true),
        ("Credit", "Customer credit balances", false),
    };

    public Result<DashboardData> Dashboard() => Run(() =>
    {
        Need("Dashboard");
        var r = new ReportRepository();
        return new DashboardData
        {
            SalesToday = r.SalesToday(), DeliveriesToday = r.DeliveriesToday(), ExpensesToday = r.ExpensesToday(),
            OutstandingCredit = r.OutstandingCredit(), LitresByProduct = r.LitresByProductToday(),
            Tanks = new TankRepository().GetAll(true)
        };
    });

    public Result<DataTable> Build(string key, DateTime from, DateTime to) => Run(() =>
    {
        Need("Reports");
        if (to.Date < from.Date) throw new BusinessException("'To' date cannot be before 'From' date.");
        var r = new ReportRepository();
        return key switch
        {
            "SalesProduct" => r.SalesByProduct(from, to),
            "SalesPump" => r.SalesByPump(from, to),
            "SalesAttendant" => r.SalesByAttendant(from, to),
            "Stock" => r.StockLevels(),
            "Deliveries" => r.DeliveriesBySupplier(from, to),
            "Shifts" => r.ShiftReconciliation(from, to),
            "Expenses" => r.ExpensesByCategory(from, to),
            "Credit" => r.CreditBalances(),
            _ => throw new BusinessException("Unknown report.")
        };
    });
}
