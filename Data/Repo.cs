namespace PetrolStationMS.Data;

public abstract class Repo
{
    readonly IDbConnection? _c;
    readonly IDbTransaction? _t;
    protected Repo(IDbConnection? c = null, IDbTransaction? t = null) { _c = c; _t = t; }

    protected T Run<T>(Func<IDbConnection, IDbTransaction?, T> f)
    {
        if (_c != null) return f(_c, _t);
        using var c = Db.Open();
        return f(c, null);
    }
}
