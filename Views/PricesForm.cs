namespace PetrolStationMS.Views;
// Manager sets fuel prices; every change is kept in history
public class PricesForm : BaseForm
{
    readonly PriceController _c = new();
    readonly DataGridView _grid = Ui.Grid();
    readonly ComboBox _product = Ui.Combo();
    readonly NumericUpDown _price = Ui.Num(1_000_000);
    readonly Label _current = new() { AutoSize = true, Margin = new Padding(8, 24, 0, 0) };

    public PricesForm() : base("Fuel Prices", 800, 520)
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(8) };
        Ui.Field(top, "Product", _product, 160); Ui.Field(top, "New price per litre", _price, 140);
        var btn = Ui.Btn("Set Price", (s, e) => SetPrice()); btn.Margin = new Padding(0, 20, 6, 0);
        top.Controls.Add(btn); top.Controls.Add(_current);
        Controls.Add(_grid); Controls.Add(top); _grid.BringToFront();

        var r = new ProductController().GetAll(true);
        if (Ui.Check(r)) foreach (var p in r.Value!) _product.Items.Add(p);
        _product.SelectedIndexChanged += (s, e) => ShowCurrent();
        Load += (s, e) => Reload();
    }

    void ShowCurrent()
    {
        _current.Text = "";
        if (_product.SelectedItem is not Product p) return;
        var r = _c.Current(p.Id);
        if (Ui.Check(r)) _current.Text = r.Value.HasValue ? $"Current price: {r.Value:N2}" : "No price set yet";
    }

    void Reload() => Ui.Bind(_grid, _c.GetHistory());

    void SetPrice()
    {
        if (_product.SelectedItem is not Product p) { Ui.Error("Select a product."); return; }
        if (!Ui.Confirm($"Set {p.Name} price to {_price.Value:N2}? Past sales keep their old price.")) return;
        if (!Ui.Check(_c.SetPrice(p.Id, _price.Value))) return;
        ShowCurrent(); Reload();
    }
}
