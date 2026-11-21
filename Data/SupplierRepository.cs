namespace PetrolStationMS.Data;
public class SupplierRepository : Repo
{
    public SupplierRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    public List<Supplier> GetAll(bool activeOnly = false) => Run((c, t) => c.Query<Supplier>(
        "SELECT * FROM suppliers WHERE (@a = FALSE OR is_active) ORDER BY name", new { a = activeOnly }, t).ToList());
    public int Insert(Supplier s) => Run((c, t) => c.ExecuteScalar<int>(
        "INSERT INTO suppliers(name, phone, email, address, is_active) VALUES (@Name, @Phone, @Email, @Address, @IsActive) RETURNING id", s, t));
    public void Update(Supplier s) => Run((c, t) => c.Execute(
        "UPDATE suppliers SET name=@Name, phone=@Phone, email=@Email, address=@Address, is_active=@IsActive WHERE id=@Id", s, t));
}
