namespace PetrolStationMS.Data;
public class SaleRepository : Repo
{
    public SaleRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = @"SELECT s.*, p.code AS pump_code, f.name AS product_name, u.full_name AS attendant_name
        FROM sales s JOIN pumps p ON p.id = s.pump_id JOIN fuel_products f ON f.id = s.product_id
        JOIN shifts h ON h.id = s.shift_id JOIN users u ON u.id = h.attendant_id";

    public List<Sale> GetRange(DateTime from, DateTime to) => Run((c, t) => c.Query<Sale>(
        Sel + " WHERE s.created_at >= @from AND s.created_at < @to ORDER BY s.created_at DESC, s.id DESC",
        new { from = from.Date, to = to.Date.AddDays(1) }, t).ToList());
    public List<Sale> GetByShift(int shiftId) => Run((c, t) => c.Query<Sale>(
        Sel + " WHERE s.shift_id=@shiftId ORDER BY s.id", new { shiftId }, t).ToList());
    public Sale? Get(int id, bool forUpdate = false) => Run((c, t) => c.QueryFirstOrDefault<Sale>(
        Sel + " WHERE s.id=@id" + (forUpdate ? " FOR UPDATE OF s" : ""), new { id }, t));
    public int Insert(int shiftId, int pumpId, int productId, decimal litres, decimal unitPrice) => Run((c, t) => c.ExecuteScalar<int>(
        @"INSERT INTO sales(shift_id, pump_id, product_id, litres, unit_price, total)
          VALUES (@shiftId, @pumpId, @productId, @litres, @unitPrice, round(@litres * @unitPrice, 2)) RETURNING id",
        new { shiftId, pumpId, productId, litres, unitPrice }, t));
    public void MarkVoid(int id, string reason, int by) => Run((c, t) => c.Execute(
        "UPDATE sales SET is_void=TRUE, void_reason=@reason, voided_by=@by, voided_at=now() WHERE id=@id", new { id, reason, by }, t));
    public int CountActivePayments(int saleId) => Run((c, t) => c.ExecuteScalar<int>(
        "SELECT count(*) FROM payments WHERE sale_id=@saleId AND NOT is_void", new { saleId }, t));
    public int TankIdOfPump(int pumpId) => Run((c, t) => c.ExecuteScalar<int>("SELECT tank_id FROM pumps WHERE id=@pumpId", new { pumpId }, t));
}
