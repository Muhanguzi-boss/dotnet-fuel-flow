namespace PetrolStationMS.Views;
public class AuditForm : DateFilterForm
{
    readonly AuditController _c = new();
    readonly TextBox _user = new() { Width = 110 };
    readonly ComboBox _action = Ui.Combo();

    public AuditForm() : base("Audit Log", 1000, 580)
    {
        _action.Width = 150;
        Bar.Controls.Add(new Label { Text = "User", AutoSize = true, Margin = new Padding(16, 6, 4, 0) });
        Bar.Controls.Add(_user);
        Bar.Controls.Add(new Label { Text = "Action", AutoSize = true, Margin = new Padding(12, 6, 4, 0) });
        Bar.Controls.Add(_action);
        _action.Items.Add("(all)");
        var a = _c.Actions(); if (Ui.Check(a)) foreach (var x in a.Value!) _action.Items.Add(x);
        _action.SelectedIndex = 0;
    }

    protected override void Reload()
    {
        var act = _action.SelectedIndex > 0 ? _action.SelectedItem as string : null;
        Ui.Bind(Grid, _c.Query(_user.Text, act, From.Value, To.Value));
    }
}
