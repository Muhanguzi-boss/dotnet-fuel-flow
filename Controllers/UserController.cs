namespace PetrolStationMS.Controllers;

// User administration: Admin only
public class UserController : BaseController
{
    readonly UserRepository _repo = new();
    public Result<List<User>> GetAll() => Run(() => { Need("Users"); return _repo.GetAll(); });
    public Result<List<Role>> GetRoles() => Run(() => _repo.GetRoles());

    static void CheckPassword(string pw) => Require(pw.Length >= 6, "Password must be at least 6 characters.");

    public Result Save(User u, string password) => Run(() =>
    {
        Need("Users");
        u.Username = Clean(u.Username); u.FullName = Clean(u.FullName);
        Require(u.Username.Length >= 3, "Username must be at least 3 characters.");
        Require(u.FullName != "", "Full name is required.");
        Require(u.RoleId > 0, "Select a role.");
        if (u.Id == 0)
        {
            CheckPassword(password);
            Require(_repo.GetByUsername(u.Username) == null, "That username is already taken.");
            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _repo.Insert(u);
            AuditController.Write("USER_ADD", $"Created user {u.Username}");
        }
        else
        {
            Require(!(u.Id == Session.UserId && !u.IsActive), "You cannot deactivate your own account.");
            _repo.Update(u);
            AuditController.Write("USER_EDIT", $"Edited user {u.Username} (role id {u.RoleId}, active {u.IsActive})");
        }
        return Result.Success();
    });

    public Result SetActive(User u, bool active) { u.IsActive = active; return Save(u, ""); }

    public Result ResetPassword(User u, string password) => Run(() =>
    {
        Need("Users");
        CheckPassword(password);
        _repo.SetPassword(u.Id, BCrypt.Net.BCrypt.HashPassword(password));
        AuditController.Write("USER_PWD_RESET", $"Reset password of {u.Username}");
        return Result.Success();
    });
}
