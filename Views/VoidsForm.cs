namespace PetrolStationMS.Views;
// Manager-only: void sales, deliveries or payments with a reason (rows are kept)
public class VoidsForm : DateFilterForm
{
    readonly SaleController _sales = new();
    readonly DeliveryController _deliveries = new();
    readonly PaymentController _payments = new();
    readonly ComboBox _type = Ui.Combo();
    readonly TextBox _reason = new() { Width = 360, MaxLength = 300 };

    public VoidsForm() : base("Void Transactions", 1000, 600)
    {
        _type.Width = 110; _type.Items.AddRange(new object[] { "Sale", "Delivery", "Payment" }); _type.SelectedIndex = 0;
        Bar.Controls.Add(new Label { Text = "Type", AutoSize = true, Margin = new Padding(16, 6, 4, 0) });
        Bar.Controls.Add(_type);
        _type.SelectedIndexChanged += (s, e) => Reload();

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(8, 8, 0, 0) };
        bottom.Controls.Add(new Label { Text = "Reason", AutoSize = true, Margin = new Padding(0, 6, 4, 0) });
        bottom.Controls.Add(_reason);
        bottom.Controls.Add(Ui.Btn("Void Selected", (s, e) => DoVoid(), 120));
        Controls.Add(bottom);
        Grid.BringToFront();
    }

    protected override void Reload()
    {
        switch (_type.SelectedItem as string)
        {
            case "Sale": Ui.Bind(Grid, _sales.GetRange(From.Value, To.Value)); break;
            case "Delivery": Ui.Bind(Grid, _deliveries.GetRange(From.Value, To.Value)); break;
            default: Ui.Bind(Grid, _payments.GetRange(From.Value, To.Value)); break;
        }
    }

    void DoVoid()
    {
        var item = Grid.CurrentRow?.DataBoundItem;
        if (item == null) { Ui.Error("Select a record to void."); return; }
        if (!Ui.Confirm("Void the selected record? The original row is kept and its effects are reversed.")) return;
        var r = item switch
        {
            Sale s => _sales.Void(s.Id, _reason.Text),
            Delivery d => _deliveries.Void(d.Id, _reason.Text),
            Payment p => _payments.Void(p.Id, _reason.Text),
            _ => Result.Fail("Unsupported record.")
        };
        if (!Ui.Check(r)) return;
        _reason.Text = "";
        Reload();
    }
}
