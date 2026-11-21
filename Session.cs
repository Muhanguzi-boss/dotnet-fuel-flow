namespace PetrolStationMS;

// Holds the logged-in user and role-based permissions
public static class Session
{
    public static User? User { get; private set; }
    public static string Role => User?.RoleName ?? "";
    public static int UserId => User?.Id ?? 0;

    public static void Start(User u) => User = u;
    public static void End() => User = null;

    // module -> roles allowed (Admin is always allowed)
    static readonly Dictionary<string, string[]> Map = new()
    {
        ["Dashboard"]      = new[] { "Manager", "Cashier" },
        ["Products"]       = new[] { "Manager" },
        ["Tanks"]          = new[] { "Manager" },
        ["Pumps"]          = new[] { "Manager" },
        ["Prices"]         = new[] { "Manager" },
        ["Users"]          = new string[0],                       // Admin only
        ["Suppliers"]      = new[] { "Manager" },
        ["Deliveries"]     = new[] { "Manager" },
        ["Shifts"]         = new[] { "Manager", "Attendant" },
        ["Sales"]          = new[] { "Manager", "Cashier" },
        ["Customers"]      = new[] { "Manager", "Cashier" },
        ["Payments"]       = new[] { "Manager", "Cashier" },
        ["Expenses"]       = new[] { "Manager", "Cashier" },
        ["Reconciliation"] = new[] { "Manager", "Cashier" },
        ["Reports"]        = new[] { "Manager" },
        ["Audit"]          = new[] { "Manager" },
        ["Voids"]          = new[] { "Manager" },
    };

    public static bool Can(string module) =>
        User != null && (Role == "Admin" || (Map.TryGetValue(module, out var roles) && roles.Contains(Role)));
}
