namespace PetrolStationMS.Views;
public class CustomersForm : CrudForm<Customer>
{
    readonly CustomerController _c = new();
    readonly TextBox _name = new() { MaxLength = 100 }, _phone = new() { MaxLength = 30 };
    readonly NumericUpDown _limit = Ui.Num();
    public CustomersForm() : base("Customers")
    {
        Ui.Field(Fields, "Name", _name, 220); Ui.Field(Fields, "Phone", _phone, 130); Ui.Field(Fields, "Credit limit", _limit, 130);
    }
    protected override Result<List<Customer>> Fetch() => _c.GetAll();
    protected override bool IsActive(Customer x) => x.IsActive;
    protected override void ShowItem(Customer? x) { _name.Text = x?.Name ?? ""; _phone.Text = x?.Phone ?? ""; _limit.Value = x?.CreditLimit ?? 0; }
    protected override Result SaveItem(Customer? cur)
    {
        var x = cur ?? new Customer();
        x.Name = _name.Text; x.Phone = _phone.Text; x.CreditLimit = _limit.Value;
        return _c.Save(x);
    }
    protected override Result Toggle(Customer x, bool active) => _c.SetActive(x, active);
}
