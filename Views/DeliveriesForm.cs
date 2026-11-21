namespace PetrolStationMS.Views;
public class DeliveriesForm : BaseForm
{
    readonly DeliveryController _c = new();
    readonly DataGridView _grid = Ui.Grid();
    readonly ComboBox _supplier = Ui.Combo(), _product = Ui.Combo(), _tank = Ui.Combo();
    readonly NumericUpDown _qty = Ui.Num(1_000_000), _cost = Ui.Num(1_000_000);
    readonly TextBox _invoice = new() { MaxLength = 50 };
    readonly DateTimePicker _when = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", Value = DateTime.Now };
    readonly DateTimePicker _from = Ui.DatePick(DateTime.Today.AddDays(-30)), _to = Ui.DatePick();

    public DeliveriesForm() : base("Fuel Deliveries", 1050, 650)
    {
        var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 6, 0, 0) };
        filter.Controls.Add(new Label { Text = "History from", AutoSize = true, Margin = new Padding(0, 6, 4, 0) });
        _from.Width = 110; _to.Width = 110;
        filter.Controls.Add(_from);
        filter.Controls.Add(new Label { Text = "to", AutoSize = true, Margin = new Padding(6, 6, 6, 0) });
        filter.Controls.Add(_to);
        filter.Controls.Add(Ui.Btn("Refresh", (s, e) => Reload(), 80));

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 8, 8, 0) };
        Ui.Field(top, "Supplier", _supplier, 180); Ui.Field(top, "Product", _product, 110); Ui.Field(top, "Tank", _tank, 160);
        Ui.Field(top, "Quantity (L)", _qty, 100); Ui.Field(top, "Unit cost", _cost, 90); Ui.Field(top, "Invoice ref", _invoice, 110);
        Ui.Field(top, "Received at", _when, 150);
        var btn = Ui.Btn("Record Delivery", (s, e) => Record(), 120); btn.Margin = new Padding(0, 20, 0, 0);
        top.Controls.Add(btn);

        Controls.Add(_grid); Controls.Add(filter); Controls.Add(top); _grid.BringToFront();

        var s1 = new SupplierController().GetAll(true); if (Ui.Check(s1)) foreach (var x in s1.Value!) _supplier.Items.Add(x);
        var p1 = new ProductController().GetAll(true); if (Ui.Check(p1)) foreach (var x in p1.Value!) _product.Items.Add(x);
        var t1 = new TankController().GetAll(true); if (Ui.Check(t1)) foreach (var x in t1.Value!) _tank.Items.Add(x);
        Load += (s, e) => Reload();
    }

    void Reload() => Ui.Bind(_grid, _c.GetRange(_from.Value, _to.Value));

    void Record()
    {
        var d = new Delivery
        {
            SupplierId = (_supplier.SelectedItem as Supplier)?.Id ?? 0, ProductId = (_product.SelectedItem as Product)?.Id ?? 0,
            TankId = (_tank.SelectedItem as Tank)?.Id ?? 0, Quantity = _qty.Value, UnitCost = _cost.Value,
            InvoiceRef = _invoice.Text, ReceivedAt = _when.Value
        };
        if (!Ui.Check(_c.Record(d))) return;
        Ui.Info("Delivery recorded and tank stock updated.");
        _qty.Value = 0; _invoice.Text = ""; _when.Value = DateTime.Now;
        Reload();
    }
}
