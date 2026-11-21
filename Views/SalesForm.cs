namespace PetrolStationMS.Views;
public class SalesForm : DateFilterForm
{
    readonly SaleController _c = new();
    public SalesForm() : base("Sales") { }
    protected override void Reload() => Ui.Bind(Grid, _c.GetRange(From.Value, To.Value));
}
