namespace PetrolStationMS.Views;
public class SuppliersForm : CrudForm<Supplier>
{
    readonly SupplierController _c = new();
    readonly TextBox _name = new() { MaxLength = 100 }, _phone = new() { MaxLength = 30 }, _email = new() { MaxLength = 100 }, _addr = new() { MaxLength = 200 };
    public SuppliersForm() : base("Suppliers")
    {
        Ui.Field(Fields, "Name", _name, 200); Ui.Field(Fields, "Phone", _phone, 130); Ui.Field(Fields, "Email", _email, 190); Ui.Field(Fields, "Address", _addr, 200);
    }
    protected override Result<List<Supplier>> Fetch() => _c.GetAll();
    protected override bool IsActive(Supplier s) => s.IsActive;
    protected override void ShowItem(Supplier? s) { _name.Text = s?.Name ?? ""; _phone.Text = s?.Phone ?? ""; _email.Text = s?.Email ?? ""; _addr.Text = s?.Address ?? ""; }
    protected override Result SaveItem(Supplier? cur)
    {
        var s = cur ?? new Supplier();
        s.Name = _name.Text; s.Phone = _phone.Text; s.Email = _email.Text; s.Address = _addr.Text;
        return _c.Save(s);
    }
    protected override Result Toggle(Supplier s, bool active) => _c.SetActive(s, active);
}
