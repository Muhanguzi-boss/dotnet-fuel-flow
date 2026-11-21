namespace PetrolStationMS.Views;
public class TanksForm : CrudForm<Tank>
{
    readonly TankController _c = new();
    readonly TextBox _code = new() { MaxLength = 20 };
    readonly ComboBox _product = Ui.Combo();
    readonly NumericUpDown _cap = Ui.Num(), _stock = Ui.Num(), _low = Ui.Num();
    public TanksForm() : base("Tanks")
    {
        Ui.Field(Fields, "Code", _code, 120); Ui.Field(Fields, "Product", _product, 140);
        Ui.Field(Fields, "Capacity (L)", _cap, 120); Ui.Field(Fields, "Current stock (L)", _stock, 120); Ui.Field(Fields, "Low-stock threshold (L)", _low, 140);
        var r = new ProductController().GetAll(true);
        if (Ui.Check(r)) foreach (var p in r.Value!) _product.Items.Add(p);
    }
    protected override Result<List<Tank>> Fetch() => _c.GetAll();
    protected override bool IsActive(Tank k) => k.IsActive;
    protected override void ShowItem(Tank? k)
    {
        _code.Text = k?.Code ?? "";
        if (k == null) _product.SelectedIndex = -1; else Ui.SelectById<Product>(_product, k.ProductId, p => p.Id);
        _cap.Value = k?.Capacity ?? 0; _stock.Value = k?.CurrentStock ?? 0; _low.Value = k?.LowThreshold ?? 0;
    }
    protected override Result SaveItem(Tank? cur)
    {
        var k = cur ?? new Tank();
        k.Code = _code.Text; k.ProductId = (_product.SelectedItem as Product)?.Id ?? 0;
        k.Capacity = _cap.Value; k.CurrentStock = _stock.Value; k.LowThreshold = _low.Value;
        return _c.Save(k);
    }
    protected override Result Toggle(Tank k, bool active) => _c.SetActive(k, active);
}
