namespace PetrolStationMS.Controllers;

public class AuditController : BaseController
{
    // Writes an audit row. Pass c/t to make it part of the caller's transaction.
    public static void Write(string action, string? details, IDbConnection? c = null, IDbTransaction? t = null)
    {
        if (c != null) { new AuditRepository(c, t).Insert(Session.User?.Id, Session.User?.Username, action, details); return; }
        try { new AuditRepository().Insert(Session.User?.Id, Session.User?.Username, action, details); }
        catch { /* auditing must never crash the app */ }
    }

    public Result<List<AuditLog>> Query(string? username, string? action, DateTime from, DateTime to) => Run(() =>
    {
        Need("Audit");
        if (to.Date < from.Date) throw new BusinessException("'To' date cannot be before 'From' date.");
        var u = Clean(username);
        return new AuditRepository().Query(u == "" ? null : u, string.IsNullOrEmpty(action) ? null : action, from, to);
    });

    public Result<List<string>> Actions() => Run(() => new AuditRepository().Actions());
}
