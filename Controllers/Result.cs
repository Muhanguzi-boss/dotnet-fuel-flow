using Npgsql;
namespace PetrolStationMS.Controllers;

public class Result
{
    public bool Ok { get; init; }
    public string Error { get; init; } = "";
    public static Result Success() => new() { Ok = true };
    public static Result Fail(string msg) => new() { Ok = false, Error = msg };
}

public class Result<T> : Result
{
    public T? Value { get; init; }
    public static Result<T> Success(T v) => new() { Ok = true, Value = v };
    public new static Result<T> Fail(string msg) => new() { Ok = false, Error = msg };
}

public class BusinessException : Exception
{
    public BusinessException(string msg) : base(msg) { }
}

public abstract class BaseController
{
    protected static Result Run(Func<Result> f)
    {
        try { return f(); }
        catch (Exception ex) { return Result.Fail(Friendly(ex)); }
    }

    protected static Result<T> Run<T>(Func<T> f)
    {
        try { return Result<T>.Success(f()); }
        catch (Exception ex) { return Result<T>.Fail(Friendly(ex)); }
    }

    static string Friendly(Exception ex) => ex switch
    {
        BusinessException b => b.Message,
        PostgresException { SqlState: "23505" } => "A record with the same unique value already exists.",
        PostgresException { SqlState: "23503" } => "This record is referenced by other data and cannot be changed this way.",
        PostgresException { SqlState: "23514" } => "A value is outside the allowed range.",
        NpgsqlException => "Cannot reach the database. Check that PostgreSQL is running and appsettings.json is correct.",
        _ => "Unexpected error: " + ex.Message
    };

    protected static void Need(string module)
    {
        if (!Session.Can(module)) throw new BusinessException("You do not have permission for this action.");
    }

    protected static void Require(bool ok, string msg) { if (!ok) throw new BusinessException(msg); }
    protected static string Clean(string? s) => (s ?? "").Trim();
}
