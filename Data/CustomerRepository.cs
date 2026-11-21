namespace PetrolStationMS.Data;
public class CustomerRepository : Repo
{
    public CustomerRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    public List<Customer> GetAll(bool activeOnly = false) => Run((c, t) => c.Query<Customer>(
        "SELECT * FROM customers WHERE (@a = FALSE OR is_active) ORDER BY name", new { a = activeOnly }, t).ToList());
    public Customer? Get(int id, bool forUpdate = false) => Run((c, t) => c.QueryFirstOrDefault<Customer>(
        "SELECT * FROM customers WHERE id=@id" + (forUpdate ? " FOR UPDATE" : ""), new { id }, t));
    public int Insert(Customer x) => Run((c, t) => c.ExecuteScalar<int>(
        "INSERT INTO customers(name, phone, credit_limit, is_active) VALUES (@Name, @Phone, @CreditLimit, @IsActive) RETURNING id", x, t));
    public void Update(Customer x) => Run((c, t) => c.Execute(
        "UPDATE customers SET name=@Name, phone=@Phone, credit_limit=@CreditLimit, is_active=@IsActive WHERE id=@Id", x, t));
    public bool AdjustBalance(int id, decimal delta) => Run((c, t) => c.Execute(
        "UPDATE customers SET balance = balance + @delta WHERE id=@id AND balance + @delta >= 0", new { id, delta }, t) == 1);
}
