namespace PetrolStationMS.Views;
public class ProductsForm : CrudForm<Product>
{
    readonly ProductController _c = new();
    readonly TextBox _code = new() { MaxLength = 10 }, _name = new() { MaxLength = 50 }, _unit = new() { MaxLength = 10 };
    public ProductsForm() : base("Fuel Products")
    {
        Ui.Field(Fields, "Code", _code, 100); Ui.Field(Fields, "Name", _name); Ui.Field(Fields, "Unit", _unit, 80);
    }
    protected override Result<List<Product>> Fetch() => _c.GetAll();
    protected override bool IsActive(Product p) => p.IsActive;
    protected override void ShowItem(Product? p) { _code.Text = p?.Code ?? ""; _name.Text = p?.Name ?? ""; _unit.Text = p?.Unit ?? "L"; }
    protected override Result SaveItem(Product? cur)
    {
        var p = cur ?? new Product();
        p.Code = _code.Text; p.Name = _name.Text; p.Unit = _unit.Text;
        return _c.Save(p);
    }
    protected override Result Toggle(Product p, bool active) => _c.SetActive(p, active);
}
