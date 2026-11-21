namespace PetrolStationMS.Views;
// Record payments for a shift/sale (Cash, Mobile Money, Card, Credit)
public class PaymentsForm : BaseForm
{
    readonly PaymentController _c = new();
    readonly ShiftController _shifts = new();
    readonly DataGridView _grid = Ui.Grid();
    readonly ComboBox _shift = Ui.Combo(), _sale = Ui.Combo(), _method = Ui.Combo(), _customer = Ui.Combo();
    readonly NumericUpDown _amount = Ui.Num();
    readonly TextBox _ref = new() { MaxLength = 80 };
    readonly Label _summary = new() { AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(10, 4, 0, 4) };

    public PaymentsForm() : base("Payments", 1050, 600)
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 8, 8, 0) };
        Ui.Field(top, "Shift", _shift, 250); Ui.Field(top, "Sale (optional)", _sale, 220); Ui.Field(top, "Method", _method, 110);
        Ui.Field(top, "Amount", _amount, 110); Ui.Field(top, "Credit customer", _customer, 160); Ui.Field(top, "Reference", _ref, 110);
        var btn = Ui.Btn("Record Payment", (s, e) => Record(), 120); btn.Margin = new Padding(0, 20, 0, 0);
        top.Controls.Add(btn);
        Controls.Add(_grid); Controls.Add(_summary); Controls.Add(top); _grid.BringToFront();

        _method.Items.AddRange(PaymentController.Methods); _method.SelectedIndex = 0;
        _method.SelectedIndexChanged += (s, e) => _customer.Enabled = (string?)_method.SelectedItem == "Credit";
        _customer.Enabled = false;
        var cu = new CustomerController().GetAll(true); if (Ui.Check(cu)) foreach (var x in cu.Value!) _customer.Items.Add(x);
        var sh = _shifts.GetRecent(); if (Ui.Check(sh)) foreach (var x in sh.Value!) _shift.Items.Add(x);
        _shift.SelectedIndexChanged += (s, e) => LoadShift();
        Load += (s, e) => { if (_shift.Items.Count > 0) _shift.SelectedIndex = 0; };
    }

    int ShiftId => (_shift.SelectedItem as Shift)?.Id ?? 0;

    void LoadShift()
    {
        _sale.Items.Clear(); _sale.Items.Add("(whole shift)");
        var sales = _c.SalesOfShift(ShiftId);
        if (Ui.Check(sales)) foreach (var x in sales.Value!.Where(v => !v.IsVoid)) _sale.Items.Add(x);
        _sale.SelectedIndex = 0;
        ShowPayments();
    }

    void ShowPayments()
    {
        Ui.Bind(_grid, _c.GetByShift(ShiftId));
        var r = _c.Reconcile(ShiftId);
        if (r.Ok) _summary.Text = $"Expected sales {r.Value!.ExpectedSales:N2}   |   Payments {r.Value.TotalPayments:N2}   |   {r.Value.Status} {Math.Abs(r.Value.Difference):N2}";
    }

    void Record()
    {
        var p = new Payment
        {
            ShiftId = ShiftId == 0 ? null : ShiftId, SaleId = (_sale.SelectedItem as Sale)?.Id,
            Method = _method.SelectedItem as string ?? "", Amount = _amount.Value, Reference = _ref.Text,
            CustomerId = (_customer.SelectedItem as Customer)?.Id
        };
        if (!Ui.Check(_c.Record(p))) return;
        _amount.Value = 0; _ref.Text = "";
        ShowPayments();
    }
}
