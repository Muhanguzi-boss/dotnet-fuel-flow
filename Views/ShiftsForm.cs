namespace PetrolStationMS.Views;
// Open a shift with opening meter readings; close it with closing readings
public class ShiftsForm : BaseForm
{
    readonly ShiftController _c = new();
    readonly DataGridView _grid = Ui.Grid(), _history = Ui.Grid();
    readonly Label _info = new() { AutoSize = true, Font = new Font(Ui.AppFont, FontStyle.Bold), Margin = new Padding(12, 8, 0, 0) };
    readonly Button _open, _close;
    BindingList<PumpReading> _readings = new();
    Shift? _openShift;

    public ShiftsForm() : base("Shifts & Pump Readings", 900, 640)
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8, 8, 8, 0) };
        _open = Ui.Btn("Open Shift", (s, e) => OpenShift(), 120);
        _close = Ui.Btn("Close Shift", (s, e) => CloseShift(), 120);
        top.Controls.AddRange(new Control[] { _open, _close, _info });

        var hist = new GroupBox { Text = "Recent shifts", Dock = DockStyle.Bottom, Height = 200 };
        hist.Controls.Add(_history);
        var hint = new Label { Dock = DockStyle.Top, Height = 24, Padding = new Padding(8, 4, 0, 0),
            Text = "Edit the highlighted meter column, then use the button above." };

        Controls.Add(_grid); Controls.Add(hint); Controls.Add(hist); Controls.Add(top); _grid.BringToFront();
        _grid.DataError += (s, e) => { Ui.Error("Enter a valid number for the meter reading."); e.ThrowException = false; };
        _grid.CellEndEdit += (s, e) => _grid.Invalidate();
        Load += (s, e) => Reload();
    }

    void Reload()
    {
        var o = _c.GetOpen();
        if (!Ui.Check(o)) return;
        _openShift = o.Value;
        var r = _openShift == null ? _c.PrepareOpening() : _c.GetReadings(_openShift.Id);
        if (!Ui.Check(r)) return;
        _readings = new BindingList<PumpReading>(r.Value!);
        _grid.DataSource = _readings;
        Ui.Tidy(_grid);
        _grid.ReadOnly = false;
        foreach (DataGridViewColumn col in _grid.Columns) col.ReadOnly = true;
        var editCol = _openShift == null ? "OpeningReading" : "ClosingReading";
        var ec = _grid.Columns[editCol]!; ec.ReadOnly = false;
        ec.DefaultCellStyle.BackColor = Color.LightYellow;

        _open.Enabled = _openShift == null; _close.Enabled = _openShift != null;
        _info.Text = _openShift == null ? "No shift is open. Enter opening readings and open a shift."
            : $"Shift #{_openShift.Id} open since {_openShift.OpenedAt:yyyy-MM-dd HH:mm} ({_openShift.AttendantName})";
        Ui.Bind(_history, _c.GetRecent());
    }

    void OpenShift()
    {
        _grid.EndEdit();
        if (!Ui.Check(_c.Open(_readings.ToList()))) return;
        Reload();
    }

    void CloseShift()
    {
        _grid.EndEdit();
        if (_openShift == null) return;
        if (!Ui.Confirm("Close this shift? Sales will be created from the readings and tank stock reduced.")) return;
        if (!Ui.Check(_c.Close(_openShift.Id, _readings.ToList()))) return;
        Ui.Info("Shift closed. Sales created and tank stock updated.");
        Reload();
    }
}
