namespace PetrolStationMS.Views;
public class LoginForm : Form
{
    readonly AuthController _auth = new();
    readonly TextBox _user = new() { Width = 240 }, _pass = new() { Width = 240, UseSystemPasswordChar = true };

    public LoginForm()
    {
        Text = "Petrol Station MS - Login"; Font = Ui.AppFont; ClientSize = new Size(300, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
        var title = new Label { Text = "Petrol Station Management", Font = new Font(Ui.AppFont.FontFamily, 13f, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = true, Left = 30, Top = 16 };
        var l1 = new Label { Text = "Username", AutoSize = true, Left = 30, Top = 60 }; _user.Left = 30; _user.Top = 80;
        var l2 = new Label { Text = "Password", AutoSize = true, Left = 30, Top = 112 }; _pass.Left = 30; _pass.Top = 132;
        var ok = new Button { Text = "Login", Left = 30, Top = 175, Width = 110, Height = 30 };
        var cancel = new Button { Text = "Exit", Left = 160, Top = 175, Width = 110, Height = 30, DialogResult = DialogResult.Cancel };
        ok.Click += (s, e) => DoLogin();
        Controls.AddRange(new Control[] { title, l1, _user, l2, _pass, ok, cancel });
        AcceptButton = ok; CancelButton = cancel;
    }

    void DoLogin()
    {
        if (!Ui.Check(_auth.Login(_user.Text, _pass.Text))) { _pass.Clear(); _pass.Focus(); return; }
        DialogResult = DialogResult.OK;
    }
}
