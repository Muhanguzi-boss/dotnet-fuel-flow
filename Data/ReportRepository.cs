namespace PetrolStationMS.Data;

// Read-only aggregate queries for the dashboard and reports
public class ReportRepository : Repo
{
    public ReportRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }

    DataTable Table(string sql, object? p = null) => Run((c, t) =>
    {
        var dt = new DataTable();
        dt.Load(c.ExecuteReader(sql, p, t));
        return dt;
    });

    public decimal SalesToday() => Run((c, t) => c.ExecuteScalar<decimal>(
        "SELECT COALESCE(SUM(total),0) FROM sales WHERE NOT is_void AND created_at::date = CURRENT_DATE", transaction: t));
    public int DeliveriesToday() => Run((c, t) => c.ExecuteScalar<int>(
        "SELECT count(*) FROM deliveries WHERE NOT is_void AND received_at::date = CURRENT_DATE", transaction: t));
    public decimal ExpensesToday() => Run((c, t) => c.ExecuteScalar<decimal>(
        "SELECT COALESCE(SUM(amount),0) FROM expenses WHERE expense_date = CURRENT_DATE", transaction: t));
    public decimal OutstandingCredit() => Run((c, t) => c.ExecuteScalar<decimal>(
        "SELECT COALESCE(SUM(balance),0) FROM customers", transaction: t));
    public DataTable LitresByProductToday() => Table(
        @"SELECT p.name AS ""Product"", COALESCE(SUM(s.litres),0) AS ""Litres"" FROM fuel_products p
          LEFT JOIN sales s ON s.product_id = p.id AND NOT s.is_void AND s.created_at::date = CURRENT_DATE
          GROUP BY p.name ORDER BY p.name");

    // ---- reports (date range, inclusive) ----
    static object Range(DateTime f, DateTime t) => new { from = f.Date, to = t.Date.AddDays(1) };

    public DataTable SalesByProduct(DateTime f, DateTime t) => Table(
        @"SELECT p.name AS ""Product"", SUM(s.litres) AS ""Litres"", SUM(s.total) AS ""Sales Value"", count(*) AS ""Sales""
          FROM sales s JOIN fuel_products p ON p.id = s.product_id
          WHERE NOT s.is_void AND s.created_at >= @from AND s.created_at < @to GROUP BY p.name ORDER BY p.name", Range(f, t));
    public DataTable SalesByPump(DateTime f, DateTime t) => Table(
        @"SELECT u.code AS ""Pump"", SUM(s.litres) AS ""Litres"", SUM(s.total) AS ""Sales Value""
          FROM sales s JOIN pumps u ON u.id = s.pump_id
          WHERE NOT s.is_void AND s.created_at >= @from AND s.created_at < @to GROUP BY u.code ORDER BY u.code", Range(f, t));
    public DataTable SalesByAttendant(DateTime f, DateTime t) => Table(
        @"SELECT a.full_name AS ""Attendant"", SUM(s.litres) AS ""Litres"", SUM(s.total) AS ""Sales Value""
          FROM sales s JOIN shifts h ON h.id = s.shift_id JOIN users a ON a.id = h.attendant_id
          WHERE NOT s.is_void AND s.created_at >= @from AND s.created_at < @to GROUP BY a.full_name ORDER BY a.full_name", Range(f, t));
    public DataTable StockLevels() => Table(
        @"SELECT k.code AS ""Tank"", p.name AS ""Product"", k.capacity AS ""Capacity"", k.current_stock AS ""Current Stock"",
          round(k.current_stock * 100 / k.capacity, 1) AS ""Fill %"", k.low_threshold AS ""Low Threshold"",
          CASE WHEN k.current_stock <= k.low_threshold THEN 'LOW' ELSE 'OK' END AS ""Status""
          FROM tanks k JOIN fuel_products p ON p.id = k.product_id ORDER BY k.code");
    public DataTable DeliveriesBySupplier(DateTime f, DateTime t) => Table(
        @"SELECT s.name AS ""Supplier"", count(*) AS ""Deliveries"", SUM(d.quantity) AS ""Litres"", SUM(d.quantity * d.unit_cost) AS ""Total Cost""
          FROM deliveries d JOIN suppliers s ON s.id = d.supplier_id
          WHERE NOT d.is_void AND d.received_at >= @from AND d.received_at < @to GROUP BY s.name ORDER BY s.name", Range(f, t));
    public DataTable ShiftReconciliation(DateTime f, DateTime t) => Table(
        @"SELECT h.id AS ""Shift"", a.full_name AS ""Attendant"", h.opened_at AS ""Opened"", h.status AS ""Status"",
          COALESCE(s.v,0) AS ""Expected Sales"", COALESCE(p.v,0) AS ""Payments"", COALESCE(p.v,0) - COALESCE(s.v,0) AS ""Difference""
          FROM shifts h JOIN users a ON a.id = h.attendant_id
          LEFT JOIN (SELECT shift_id, SUM(total) v FROM sales WHERE NOT is_void GROUP BY shift_id) s ON s.shift_id = h.id
          LEFT JOIN (SELECT shift_id, SUM(amount) v FROM payments WHERE NOT is_void AND kind='Sale' GROUP BY shift_id) p ON p.shift_id = h.id
          WHERE h.opened_at >= @from AND h.opened_at < @to ORDER BY h.id DESC", Range(f, t));
    public DataTable ExpensesByCategory(DateTime f, DateTime t) => Table(
        @"SELECT category AS ""Category"", count(*) AS ""Entries"", SUM(amount) AS ""Total""
          FROM expenses WHERE expense_date >= @f AND expense_date <= @t GROUP BY category ORDER BY category",
        new { f = f.Date, t = t.Date });
    public DataTable CreditBalances() => Table(
        @"SELECT name AS ""Customer"", credit_limit AS ""Credit Limit"", balance AS ""Balance"", credit_limit - balance AS ""Available""
          FROM customers ORDER BY balance DESC, name");
}
