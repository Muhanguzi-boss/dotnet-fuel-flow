namespace PetrolStationMS.Data;
public class PaymentRepository : Repo
{
    public PaymentRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = @"SELECT p.*, COALESCE(c.name, '') AS customer_name, u.full_name AS recorded_by_name
        FROM payments p LEFT JOIN customers c ON c.id = p.customer_id JOIN users u ON u.id = p.recorded_by";

    public List<Payment> GetByShift(int shiftId) => Run((c, t) => c.Query<Payment>(
        Sel + " WHERE p.shift_id=@shiftId ORDER BY p.id DESC", new { shiftId }, t).ToList());
    public List<Payment> GetRange(DateTime from, DateTime to) => Run((c, t) => c.Query<Payment>(
        Sel + " WHERE p.paid_at >= @from AND p.paid_at < @to ORDER BY p.id DESC",
        new { from = from.Date, to = to.Date.AddDays(1) }, t).ToList());
    public Payment? Get(int id, bool forUpdate = false) => Run((c, t) => c.QueryFirstOrDefault<Payment>(
        Sel + " WHERE p.id=@id" + (forUpdate ? " FOR UPDATE OF p" : ""), new { id }, t));
    public int Insert(Payment p) => Run((c, t) => c.ExecuteScalar<int>(
        @"INSERT INTO payments(shift_id, sale_id, customer_id, kind, method, amount, reference, recorded_by)
          VALUES (@ShiftId, @SaleId, @CustomerId, @Kind, @Method, @Amount, @Reference, @RecordedBy) RETURNING id", p, t));
    public void MarkVoid(int id, string reason, int by) => Run((c, t) => c.Execute(
        "UPDATE payments SET is_void=TRUE, void_reason=@reason, voided_by=@by, voided_at=now() WHERE id=@id", new { id, reason, by }, t));
    public decimal ExpectedSales(int shiftId) => Run((c, t) => c.ExecuteScalar<decimal>(
        "SELECT COALESCE(SUM(total),0) FROM sales WHERE shift_id=@shiftId AND NOT is_void", new { shiftId }, t));
    // Payments for a shift grouped by method (sale payments only, not voided)
    public DataTable ByMethod(int shiftId) => Run((c, t) =>
    {
        var dt = new DataTable();
        dt.Load(c.ExecuteReader(@"SELECT method AS ""Method"", SUM(amount) AS ""Amount"" FROM payments
            WHERE shift_id=@shiftId AND kind='Sale' AND NOT is_void GROUP BY method ORDER BY method", new { shiftId }, t));
        return dt;
    });
}
