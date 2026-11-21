namespace PetrolStationMS.Data;
public class PriceRepository : Repo
{
    public PriceRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = @"SELECT f.*, p.name AS product_name, u.full_name AS set_by_name
        FROM fuel_prices f JOIN fuel_products p ON p.id = f.product_id JOIN users u ON u.id = f.set_by";

    public List<PriceHistory> GetHistory() => Run((c, t) => c.Query<PriceHistory>(
        Sel + " ORDER BY f.effective_from DESC, f.id DESC", transaction: t).ToList());
    // Price in effect for a product at a given moment (null if none)
    public decimal? GetPriceAt(int productId, DateTime at) => Run((c, t) => c.ExecuteScalar<decimal?>(
        @"SELECT price FROM fuel_prices WHERE product_id=@productId AND effective_from <= @at
          ORDER BY effective_from DESC, id DESC LIMIT 1", new { productId, at }, t));
    public void Insert(int productId, decimal price, int setBy) => Run((c, t) => c.Execute(
        "INSERT INTO fuel_prices(product_id, price, effective_from, set_by) VALUES (@productId, @price, now(), @setBy)",
        new { productId, price, setBy }, t));
}
