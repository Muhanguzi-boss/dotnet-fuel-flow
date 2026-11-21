namespace PetrolStationMS.Data;
public class TankRepository : Repo
{
    public TankRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = "SELECT k.*, p.name AS product_name FROM tanks k JOIN fuel_products p ON p.id = k.product_id";

    public List<Tank> GetAll(bool activeOnly = false) => Run((c, t) => c.Query<Tank>(
        Sel + " WHERE (@a = FALSE OR k.is_active) ORDER BY k.code", new { a = activeOnly }, t).ToList());
    // forUpdate locks the row for the rest of the transaction
    public Tank? Get(int id, bool forUpdate = false) => Run((c, t) => c.QueryFirstOrDefault<Tank>(
        Sel + " WHERE k.id=@id" + (forUpdate ? " FOR UPDATE OF k" : ""), new { id }, t));
    public int Insert(Tank k) => Run((c, t) => c.ExecuteScalar<int>(
        @"INSERT INTO tanks(code, product_id, capacity, current_stock, low_threshold, is_active)
          VALUES (@Code, @ProductId, @Capacity, @CurrentStock, @LowThreshold, @IsActive) RETURNING id", k, t));
    public void Update(Tank k) => Run((c, t) => c.Execute(
        @"UPDATE tanks SET code=@Code, product_id=@ProductId, capacity=@Capacity, current_stock=@CurrentStock,
          low_threshold=@LowThreshold, is_active=@IsActive WHERE id=@Id", k, t));
    // Atomic stock change; returns false if it would go below 0 or above capacity
    public bool AdjustStock(int id, decimal delta) => Run((c, t) => c.Execute(
        @"UPDATE tanks SET current_stock = current_stock + @delta
          WHERE id=@id AND current_stock + @delta >= 0 AND current_stock + @delta <= capacity",
        new { id, delta }, t) == 1);
}
