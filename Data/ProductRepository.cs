namespace PetrolStationMS.Data;
public class ProductRepository : Repo
{
    public ProductRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    public List<Product> GetAll(bool activeOnly = false) => Run((c, t) => c.Query<Product>(
        "SELECT * FROM fuel_products WHERE (@a = FALSE OR is_active) ORDER BY name", new { a = activeOnly }, t).ToList());
    public Product? Get(int id) => Run((c, t) => c.QueryFirstOrDefault<Product>("SELECT * FROM fuel_products WHERE id=@id", new { id }, t));
    public int Insert(Product p) => Run((c, t) => c.ExecuteScalar<int>(
        "INSERT INTO fuel_products(code, name, unit, is_active) VALUES (@Code, @Name, @Unit, @IsActive) RETURNING id", p, t));
    public void Update(Product p) => Run((c, t) => c.Execute(
        "UPDATE fuel_products SET code=@Code, name=@Name, unit=@Unit, is_active=@IsActive WHERE id=@Id", p, t));
}
