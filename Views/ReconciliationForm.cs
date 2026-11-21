namespace PetrolStationMS.Views;
public class ReconciliationForm : BaseForm
{
    readonly PaymentController _c = new();
    readonly DataGridView _grid = Ui.Grid();
    readonly ComboBox _shift = Ui.Combo();
    readonly Label _expected = Big(), _paid = Big(), _diff = Big(), _status = Big();

    static Label Big() => new() { AutoSize = true, Font = new Font(Ui.AppFont.FontFamily, 12f, FontStyle.Bold), Margin = new Padding(0, 0, 24, 0) };

    public ReconciliationForm() : base("Shift Reconciliation", 760, 480)
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 66, Padding = new Padding(8, 8, 8, 0) };
        Ui.Field(top, "Shift", _shift, 300);
        top.Controls.Add(new Label { Text = "(Payments are shown by method below)", AutoSize = true, Margin = new Padding(8, 26, 0, 0), ForeColor = Color.Gray });
        var sums = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(10, 6, 0, 0) };
        sums.Controls.AddRange(new Control[] { _expected, _paid, _diff, _status });
        Controls.Add(_grid); Controls.Add(sums); Controls.Add(top); _grid.BringToFront();

        var sh = new ShiftController().GetRecent(); if (Ui.Check(sh)) foreach (var x in sh.Value!) _shift.Items.Add(x);
        _shift.SelectedIndexChanged += (s, e) => ShowShift();
        Load += (s, e) => { if (_shift.Items.Count > 0) _shift.SelectedIndex = 0; };
    }

    void ShowShift()
    {
        if (_shift.SelectedItem is not Shift sh) return;
        var r = _c.Reconcile(sh.Id);
        if (!Ui.Check(r)) return;
        var v = r.Value!;
        _expected.Text = $"Expected: {v.ExpectedSales:N2}"; _paid.Text = $"Paid: {v.TotalPayments:N2}";
        _diff.Text = $"Difference: {v.Difference:N2}"; _status.Text = v.Status;
        _status.ForeColor = v.Status == "Balanced" ? Color.DarkGreen : v.Status == "Shortage" ? Color.Firebrick : Color.DarkOrange;
        _grid.DataSource = v.ByMethod; Ui.Tidy(_grid);
    }
}
