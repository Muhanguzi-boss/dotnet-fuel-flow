namespace PetrolStationMS.Data;
public class AuditRepository : Repo
{
    public AuditRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    public void Insert(int? userId, string? username, string action, string? details) => Run((c, t) => c.Execute(
        "INSERT INTO audit_logs(user_id, username, action, details) VALUES (@userId, @username, @action, @details)",
        new { userId, username, action, details }, t));
    public List<AuditLog> Query(string? username, string? action, DateTime from, DateTime to) => Run((c, t) => c.Query<AuditLog>(
        @"SELECT * FROM audit_logs WHERE created_at >= @from AND created_at < @to
          AND (@username IS NULL OR username ILIKE '%' || @username || '%')
          AND (@action IS NULL OR action = @action)
          ORDER BY id DESC LIMIT 2000",
        new { username, action, from = from.Date, to = to.Date.AddDays(1) }, t).ToList());
    public List<string> Actions() => Run((c, t) => c.Query<string>("SELECT DISTINCT action FROM audit_logs ORDER BY 1", transaction: t).ToList());
}
