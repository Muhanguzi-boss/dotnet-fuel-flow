namespace PetrolStationMS.Data;
public class PumpRepository : Repo
{
    public PumpRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = @"SELECT u.*, k.code AS tank_code, k.product_id, p.name AS product_name
        FROM pumps u JOIN tanks k ON k.id = u.tank_id JOIN fuel_products p ON p.id = k.product_id";

    public List<Pump> GetAll(bool activeOnly = false) => Run((c, t) => c.Query<Pump>(
        Sel + " WHERE (@a = FALSE OR u.status = 'Active') ORDER BY u.code", new { a = activeOnly }, t).ToList());
    public Pump? Get(int id) => Run((c, t) => c.QueryFirstOrDefault<Pump>(Sel + " WHERE u.id=@id", new { id }, t));
    public int Insert(Pump p) => Run((c, t) => c.ExecuteScalar<int>(
        "INSERT INTO pumps(code, tank_id, status) VALUES (@Code, @TankId, @Status) RETURNING id", p, t));
    public void Update(Pump p) => Run((c, t) => c.Execute(
        "UPDATE pumps SET code=@Code, tank_id=@TankId, status=@Status WHERE id=@Id", p, t));
}
