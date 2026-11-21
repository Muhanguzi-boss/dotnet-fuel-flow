namespace PetrolStationMS.Data;
public class DeliveryRepository : Repo
{
    public DeliveryRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = @"SELECT d.*, s.name AS supplier_name, p.name AS product_name, k.code AS tank_code, u.full_name AS received_by_name
        FROM deliveries d JOIN suppliers s ON s.id = d.supplier_id JOIN fuel_products p ON p.id = d.product_id
        JOIN tanks k ON k.id = d.tank_id JOIN users u ON u.id = d.received_by";

    public List<Delivery> GetRange(DateTime from, DateTime to) => Run((c, t) => c.Query<Delivery>(
        Sel + " WHERE d.received_at >= @from AND d.received_at < @to ORDER BY d.received_at DESC",
        new { from = from.Date, to = to.Date.AddDays(1) }, t).ToList());
    public Delivery? Get(int id, bool forUpdate = false) => Run((c, t) => c.QueryFirstOrDefault<Delivery>(
        Sel + " WHERE d.id=@id" + (forUpdate ? " FOR UPDATE OF d" : ""), new { id }, t));
    public int Insert(Delivery d) => Run((c, t) => c.ExecuteScalar<int>(
        @"INSERT INTO deliveries(supplier_id, product_id, tank_id, quantity, unit_cost, invoice_ref, received_by, received_at)
          VALUES (@SupplierId, @ProductId, @TankId, @Quantity, @UnitCost, @InvoiceRef, @ReceivedBy, @ReceivedAt) RETURNING id", d, t));
    public void MarkVoid(int id, string reason, int by) => Run((c, t) => c.Execute(
        "UPDATE deliveries SET is_void=TRUE, void_reason=@reason, voided_by=@by, voided_at=now() WHERE id=@id",
        new { id, reason, by }, t));
}
