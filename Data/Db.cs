using System.Text.Json;
namespace PetrolStationMS.Data;

// Connection factory + transaction helper
public static class Db
{
    static string _cs = "";

    // Reads ConnectionStrings:Default from appsettings.json (next to the exe)
    public static void LoadConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("appsettings.json not found. Copy appsettings.example.json to appsettings.json and set the password.");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        _cs = doc.RootElement.GetProperty("ConnectionStrings").GetProperty("Default").GetString() ?? "";
        DefaultTypeMap.MatchNamesWithUnderscores = true;   // snake_case columns -> PascalCase props
    }

    public static NpgsqlConnection Open()
    {
        var c = new NpgsqlConnection(_cs);
        c.Open();
        return c;
    }

    // Runs work inside one transaction; commits on success, rolls back on any exception
    public static T InTransaction<T>(Func<IDbConnection, IDbTransaction, T> work)
    {
        using var c = Open();
        using var t = c.BeginTransaction();
        try { var r = work(c, t); t.Commit(); return r; }
        catch { t.Rollback(); throw; }
    }
}
