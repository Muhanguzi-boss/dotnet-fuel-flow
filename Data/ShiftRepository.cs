namespace PetrolStationMS.Data;
public class ShiftRepository : Repo
{
    public ShiftRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = "SELECT s.*, u.full_name AS attendant_name FROM shifts s JOIN users u ON u.id = s.attendant_id";

    public Shift? GetOpen() => Run((c, t) => c.QueryFirstOrDefault<Shift>(Sel + " WHERE s.status='Open'", transaction: t));
    public Shift? Get(int id, bool forUpdate = false) => Run((c, t) => c.QueryFirstOrDefault<Shift>(
        Sel + " WHERE s.id=@id" + (forUpdate ? " FOR UPDATE OF s" : ""), new { id }, t));
    public List<Shift> GetRecent(int n = 100) => Run((c, t) => c.Query<Shift>(
        Sel + " ORDER BY s.id DESC LIMIT @n", new { n }, t).ToList());
    public int Insert(int attendantId) => Run((c, t) => c.ExecuteScalar<int>(
        "INSERT INTO shifts(attendant_id) VALUES (@attendantId) RETURNING id", new { attendantId }, t));
    public void Close(int id) => Run((c, t) => c.Execute(
        "UPDATE shifts SET status='Closed', closed_at=now() WHERE id=@id", new { id }, t));

    // ---- pump readings ----
    const string RSel = @"SELECT r.*, p.code AS pump_code, f.name AS product_name FROM pump_readings r
        JOIN pumps p ON p.id = r.pump_id JOIN tanks k ON k.id = p.tank_id JOIN fuel_products f ON f.id = k.product_id";
    public List<PumpReading> GetReadings(int shiftId) => Run((c, t) => c.Query<PumpReading>(
        RSel + " WHERE r.shift_id=@shiftId ORDER BY p.code", new { shiftId }, t).ToList());
    public void InsertReading(int shiftId, int pumpId, decimal opening) => Run((c, t) => c.Execute(
        "INSERT INTO pump_readings(shift_id, pump_id, opening_reading) VALUES (@shiftId, @pumpId, @opening)",
        new { shiftId, pumpId, opening }, t));
    public void SetClosing(int readingId, decimal closing) => Run((c, t) => c.Execute(
        "UPDATE pump_readings SET closing_reading=@closing WHERE id=@readingId", new { readingId, closing }, t));
    // Last closing meter value of a pump from earlier shifts (0 if none)
    public decimal LastClosing(int pumpId) => Run((c, t) => c.ExecuteScalar<decimal?>(
        @"SELECT closing_reading FROM pump_readings WHERE pump_id=@pumpId AND closing_reading IS NOT NULL
          ORDER BY shift_id DESC LIMIT 1", new { pumpId }, t) ?? 0);
}
