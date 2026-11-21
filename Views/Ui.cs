using System.Text.RegularExpressions;
namespace PetrolStationMS.Views;

// Shared UI helpers (no business logic)
public static class Ui
{
    public static readonly Font AppFont = new("Segoe UI", 9.5f);
    public static readonly Color Accent = Color.FromArgb(0, 102, 153);

    public static void Error(string msg) => MessageBox.Show(msg, "Petrol Station MS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Info(string msg) => MessageBox.Show(msg, "Petrol Station MS", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static bool Confirm(string msg) =>
        MessageBox.Show(msg, "Petrol Station MS", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    // Shows the error if the result failed; true when OK
    public static bool Check(Result r)
    {
        if (!r.Ok) Error(r.Error);
        return r.Ok;
    }

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None, AllowUserToResizeRows = false, EnableHeadersVisualStyles = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            AutoGenerateColumns = true
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Accent;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font(AppFont, FontStyle.Bold);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 251);
        return g;
    }

    public static void Bind<T>(DataGridView g, Result<List<T>> r)
    {
        if (!Check(r)) return;
        g.DataSource = new BindingSource { DataSource = r.Value };
        Tidy(g);
    }

    public static void Bind(DataGridView g, Result<DataTable> r)
    {
        if (!Check(r)) return;
        g.DataSource = r.Value;
        Tidy(g);
    }

    // Hide id columns, split PascalCase headers, format numbers and dates
    public static void Tidy(DataGridView g)
    {
        foreach (DataGridViewColumn col in g.Columns)
        {
            if (col.Name == "Id" || col.Name == "PasswordHash" || Regex.IsMatch(col.Name, "[a-z]Id$") || col.Name == "VoidedBy"
                || col.Name == "IsLow") col.Visible = false;
            col.HeaderText = Regex.Replace(col.HeaderText, "(?<=[a-z])(?=[A-Z])", " ");
            var t = Nullable.GetUnderlyingType(col.ValueType ?? typeof(object)) ?? col.ValueType ?? typeof(object);
            if (t == typeof(decimal) || t == typeof(double))
            {
                col.DefaultCellStyle.Format = "N2";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            else if (t == typeof(DateTime)) col.DefaultCellStyle.Format = col.Name.EndsWith("Date") ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm";
        }
    }

    // Label above a control, added to a flow panel
    public static void Field(FlowLayoutPanel panel, string label, Control c, int width = 170)
    {
        var p = new Panel { Width = width + 10, Height = 52, Margin = new Padding(0, 0, 6, 0) };
        p.Controls.Add(new Label { Text = label, AutoSize = true, Left = 0, Top = 0 });
        c.Left = 0; c.Top = 20; c.Width = width;
        p.Controls.Add(c);
        panel.Controls.Add(p);
    }

    public static Button Btn(string text, EventHandler onClick, int width = 110)
    {
        var b = new Button { Text = text, Width = width, Height = 30, FlatStyle = FlatStyle.System, Margin = new Padding(0, 0, 6, 0) };
        b.Click += onClick;
        return b;
    }

    public static ComboBox Combo() => new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public static NumericUpDown Num(decimal max = 1_000_000_000, int decimals = 2) => new()
    {
        Minimum = 0, Maximum = max, DecimalPlaces = decimals, ThousandsSeparator = true
    };

    public static DateTimePicker DatePick(DateTime? v = null) => new()
    {
        Format = DateTimePickerFormat.Short, Value = v ?? DateTime.Today
    };

    // Simple input dialog; returns null if cancelled
    public static string? Prompt(string title, string label, bool password = false)
    {
        using var f = new Form
        {
            Text = title, FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false, MinimizeBox = false, ClientSize = new Size(360, 120), Font = AppFont
        };
        var l = new Label { Text = label, Left = 12, Top = 12, AutoSize = true };
        var t = new TextBox { Left = 12, Top = 36, Width = 336, UseSystemPasswordChar = password };
        var ok = new Button { Text = "OK", Left = 176, Top = 76, Width = 80, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", Left = 268, Top = 76, Width = 80, DialogResult = DialogResult.Cancel };
        f.Controls.AddRange(new Control[] { l, t, ok, cancel });
        f.AcceptButton = ok; f.CancelButton = cancel;
        return f.ShowDialog() == DialogResult.OK ? t.Text : null;
    }

    // Selects an item in a combo by its Id property value
    public static void SelectById<T>(ComboBox cb, int id, Func<T, int> getId)
    {
        for (int i = 0; i < cb.Items.Count; i++)
            if (cb.Items[i] is T item && getId(item) == id) { cb.SelectedIndex = i; return; }
        cb.SelectedIndex = -1;
    }
}

// Base form: consistent font, size, position
public class BaseForm : Form
{
    public BaseForm(string title, int w = 980, int h = 620)
    {
        Text = title; Font = Ui.AppFont; ClientSize = new Size(w, h);
        StartPosition = FormStartPosition.CenterParent; MinimumSize = new Size(Math.Min(w, 700), Math.Min(h, 450));
        ShowIcon = false; ShowInTaskbar = false;
    }
}
