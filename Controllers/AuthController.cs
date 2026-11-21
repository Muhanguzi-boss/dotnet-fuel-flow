namespace PetrolStationMS.Controllers;

public class AuthController : BaseController
{
    public Result<User> Login(string username, string password)
    {
        try
        {
            username = Clean(username);
            if (username == "" || password == "") return Result<User>.Fail("Enter both username and password.");
            var u = new UserRepository().GetByUsername(username);
            if (u == null || !u.IsActive || !BCrypt.Net.BCrypt.Verify(password, u.PasswordHash))
            {
                AuditController.Write("LOGIN_FAILED", $"Failed login for '{username}'");
                return Result<User>.Fail("Invalid username or password, or the account is deactivated.");
            }
            Session.Start(u);
            AuditController.Write("LOGIN", $"{u.Username} ({u.RoleName}) logged in");
            return Result<User>.Success(u);
        }
        catch (Exception ex) { return Result<User>.Fail(ex is NpgsqlException ? "Cannot reach the database. Check PostgreSQL and appsettings.json." : ex.Message); }
    }

    public void Logout()
    {
        AuditController.Write("LOGOUT", $"{Session.User?.Username} logged out");
        Session.End();
    }
}
