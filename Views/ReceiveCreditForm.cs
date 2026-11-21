namespace PetrolStationMS.Views;
public class ReceiveCreditForm : BaseForm
{
    readonly PaymentController _c = new();
    readonly CustomerController _cust = new();
    readonly DataGridView _grid = Ui.Grid();
    readonly ComboBox _customer = Ui.Combo(), _method = Ui.Combo();
    readonly NumericUpDown _amount = Ui.Num();
    readonly TextBox _ref = new() { MaxLength = 80 };

    public ReceiveCreditForm() : base("Receive Credit Payment", 850, 520)
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 8, 8, 8) };
        Ui.Field(top, "Customer", _customer, 220); Ui.Field(top, "Method", _method, 120); Ui.Field(top, "Amount", _amount, 120); Ui.Field(top, "Reference", _ref, 130);
        var btn = Ui.Btn("Receive Payment", (s, e) => Receive(), 130); btn.Margin = new Padding(0, 20, 0, 0);
        top.Controls.Add(btn);
        Controls.Add(_grid); Controls.Add(top); _grid.BringToFront();
        _method.Items.AddRange(PaymentController.ReceiptMethods); _method.SelectedIndex = 0;
        Load += (s, e) => Reload();
    }

    void Reload()
    {
        var r = _cust.GetAll();
        if (!Ui.Check(r)) return;
        _customer.Items.Clear();
        foreach (var x in r.Value!.Where(v => v.IsActive)) _customer.Items.Add(x);
        Ui.Bind(_grid, r);
    }

    void Receive()
    {
        if (!Ui.Check(_c.ReceiveCredit((_customer.SelectedItem as Customer)?.Id ?? 0, _method.SelectedItem as string ?? "", _amount.Value, _ref.Text))) return;
        Ui.Info("Payment received; customer balance reduced.");
        _amount.Value = 0; _ref.Text = "";
        Reload();
    }
}
