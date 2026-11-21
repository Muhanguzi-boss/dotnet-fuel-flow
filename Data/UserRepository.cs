namespace PetrolStationMS.Data;
public class UserRepository : Repo
{
    public UserRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    const string Sel = "SELECT u.*, r.name AS role_name FROM users u JOIN roles r ON r.id = u.role_id";

    public List<User> GetAll() => Run((c, t) => c.Query<User>(Sel + " ORDER BY u.username", transaction: t).ToList());
    public User? GetByUsername(string username) => Run((c, t) =>
        c.QueryFirstOrDefault<User>(Sel + " WHERE lower(u.username) = lower(@username)", new { username }, t));
    public List<Role> GetRoles() => Run((c, t) => c.Query<Role>("SELECT * FROM roles ORDER BY id", transaction: t).ToList());
    public int Insert(User u) => Run((c, t) => c.ExecuteScalar<int>(
        @"INSERT INTO users(username, password_hash, full_name, role_id, is_active)
          VALUES (@Username, @PasswordHash, @FullName, @RoleId, @IsActive) RETURNING id", u, t));
    public void Update(User u) => Run((c, t) => c.Execute(
        "UPDATE users SET full_name=@FullName, role_id=@RoleId, is_active=@IsActive WHERE id=@Id", u, t));
    public void SetPassword(int id, string hash) => Run((c, t) => c.Execute(
        "UPDATE users SET password_hash=@hash WHERE id=@id", new { id, hash }, t));
}
