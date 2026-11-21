namespace PetrolStationMS.Views;

// Base for master-data screens: grid on top, edit panel at bottom, Add/Save/Deactivate buttons.
// Subclasses supply data loading, field mapping and the controller calls.
public abstract class CrudForm<T> : BaseForm where T : class
{
    protected readonly DataGridView Grid = Ui.Grid();
    protected readonly FlowLayoutPanel Fields = new() { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(8, 8, 8, 0) };
    protected readonly FlowLayoutPanel Buttons = new() { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 6, 8, 0) };
    readonly Button _btnToggle;
    readonly Label _status = new() { AutoSize = true, Margin = new Padding(12, 8, 0, 0), ForeColor = Color.DarkGreen };
    protected T? Current;

    protected CrudForm(string title, bool canToggle = true) : base(title)
    {
        var box = new GroupBox { Text = "Details", Dock = DockStyle.Bottom, Height = 190, Padding = new Padding(4) };
        box.Controls.Add(Buttons);
        box.Controls.Add(Fields);
        Controls.Add(Grid);
        Controls.Add(box);
        Grid.BringToFront();

        Buttons.Controls.Add(Ui.Btn("New", (s, e) => NewItem()));
        Buttons.Controls.Add(Ui.Btn("Save", (s, e) => DoSave()));
        _btnToggle = Ui.Btn("Deactivate", (s, e) => DoToggle());
        if (canToggle) Buttons.Controls.Add(_btnToggle);
        Buttons.Controls.Add(_status);

        // CurrentCellChanged (not SelectionChanged, which can fire before CurrentRow updates) keeps Current in sync;
        // CellClick also covers re-clicking the same row after "New"
        Grid.CurrentCellChanged += (s, e) => Select(Grid.CurrentRow?.DataBoundItem as T);
        Grid.CellClick += (s, e) => { if (e.RowIndex >= 0) Select(Grid.Rows[e.RowIndex].DataBoundItem as T); };
        Load += (s, e) => Reload();
    }

    protected abstract Result<List<T>> Fetch();
    protected abstract void ShowItem(T? item);         // item null = blank form
    protected abstract Result SaveItem(T? current);    // current null = create
    protected virtual bool IsActive(T item) => true;
    protected virtual Result Toggle(T item, bool active) => Result.Fail("Not supported.");

    protected void Reload()
    {
        var id = Grid.CurrentRow?.Index ?? -1;
        Ui.Bind(Grid, Fetch());
        if (Grid.Rows.Count > 0 && id >= 0) Grid.CurrentCell = Grid.Rows[Math.Min(id, Grid.Rows.Count - 1)].Cells.Cast<DataGridViewCell>().First(c => c.Visible);
        else { Current = null; ShowItem(null); }
    }

    void Select(T? item)
    {
        Current = item;
        ShowItem(item);
        _btnToggle.Text = item != null && !IsActive(item) ? "Activate" : "Deactivate";
        _btnToggle.Enabled = item != null;
    }

    void NewItem() { Grid.ClearSelection(); Select(null); _status.Text = ""; }

    void DoSave()
    {
        if (!Ui.Check(SaveItem(Current))) return;
        _status.Text = "Saved.";
        Reload();
    }

    void DoToggle()
    {
        if (Current == null) return;
        var activate = !IsActive(Current);
        if (!Ui.Confirm(activate ? "Activate this record?" : "Deactivate this record?")) return;
        if (!Ui.Check(Toggle(Current, activate))) return;
        _status.Text = activate ? "Activated." : "Deactivated.";
        Reload();
    }
}
