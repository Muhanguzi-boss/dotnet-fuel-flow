namespace PetrolStationMS.Views;
public class PumpsForm : CrudForm<Pump>
{
    readonly PumpController _c = new();
    readonly TextBox _code = new() { MaxLength = 20 };
    readonly ComboBox _tank = Ui.Combo(), _status = Ui.Combo();
    public PumpsForm() : base("Pumps / Nozzles", canToggle: false)
    {
        Ui.Field(Fields, "Pump / nozzle code", _code, 140); Ui.Field(Fields, "Linked tank", _tank, 200); Ui.Field(Fields, "Status", _status, 130);
        _status.Items.AddRange(PumpController.Statuses);
        var r = new TankController().GetAll(true);
        if (Ui.Check(r)) foreach (var k in r.Value!) _tank.Items.Add(k);
    }
    protected override Result<List<Pump>> Fetch() => _c.GetAll();
    protected override void ShowItem(Pump? p)
    {
        _code.Text = p?.Code ?? "";
        if (p == null) _tank.SelectedIndex = -1; else Ui.SelectById<Tank>(_tank, p.TankId, k => k.Id);
        _status.SelectedItem = p?.Status ?? "Active";
    }
    protected override Result SaveItem(Pump? cur)
    {
        var p = cur ?? new Pump();
        p.Code = _code.Text; p.TankId = (_tank.SelectedItem as Tank)?.Id ?? 0; p.Status = _status.SelectedItem as string ?? "";
        return _c.Save(p);
    }
}
