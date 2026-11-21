namespace PetrolStationMS.Views;
public class UsersForm : CrudForm<User>
{
    readonly UserController _c = new();
    readonly TextBox _username = new() { MaxLength = 50 }, _full = new() { MaxLength = 100 }, _pwd = new() { UseSystemPasswordChar = true };
    readonly ComboBox _role = Ui.Combo();
    public UsersForm() : base("User Administration")
    {
        Ui.Field(Fields, "Username", _username, 130); Ui.Field(Fields, "Full name", _full, 190);
        Ui.Field(Fields, "Role", _role, 120); Ui.Field(Fields, "Password (new users)", _pwd, 140);
        Buttons.Controls.Add(Ui.Btn("Reset Password", (s, e) => ResetPwd(), 120));
        var r = _c.GetRoles();
        if (Ui.Check(r)) foreach (var x in r.Value!) _role.Items.Add(x.Name);
    }
    protected override Result<List<User>> Fetch() => _c.GetAll();
    protected override bool IsActive(User u) => u.IsActive;
    protected override void ShowItem(User? u)
    {
        _username.Text = u?.Username ?? ""; _username.Enabled = u == null;
        _full.Text = u?.FullName ?? ""; _pwd.Text = ""; _pwd.Enabled = u == null;
        _role.SelectedItem = u?.RoleName;
    }
    protected override Result SaveItem(User? cur)
    {
        var u = cur ?? new User();
        u.Username = _username.Text; u.FullName = _full.Text;
        var roles = _c.GetRoles();
        if (!roles.Ok) return roles;
        u.RoleId = roles.Value!.FirstOrDefault(r => r.Name == _role.SelectedItem as string)?.Id ?? 0;
        return _c.Save(u, _pwd.Text);
    }
    protected override Result Toggle(User u, bool active) => _c.SetActive(u, active);

    void ResetPwd()
    {
        if (Current == null) { Ui.Error("Select a user first."); return; }
        var pw = Ui.Prompt("Reset password", $"New password for {Current.Username}:", true);
        if (pw == null) return;
        if (Ui.Check(_c.ResetPassword(Current, pw))) Ui.Info("Password reset.");
    }
}
