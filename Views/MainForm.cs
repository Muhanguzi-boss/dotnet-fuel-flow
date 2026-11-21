namespace PetrolStationMS.Views;
// Main window: role-based menu + dashboard
public class MainForm : Form
{
    readonly MenuStrip _menu = new();
    readonly Label _sales = Card(), _deliveries = Card(), _expenses = Card(), _credit = Card();
    readonly DataGridView _litres = Ui.Grid(), _tanks = Ui.Grid();
    readonly Label _welcome = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Ui.AppFont.FontFamily, 14f) };
    readonly Panel _dash = new() { Dock = DockStyle.Fill };
    public bool LogoutRequested { get; private set; }

    static Label Card() => new() { AutoSize = false, Font = new Font(Ui.AppFont.FontFamily, 14f, FontStyle.Bold), ForeColor = Ui.Accent, TextAlign = ContentAlignment.MiddleLeft };

    public MainForm()
    {
        Text = $"Petrol Station Management System - {Session.User!.FullName} ({Session.Role})";
        Font = Ui.AppFont; ClientSize = new Size(1050, 640); StartPosition = FormStartPosition.CenterScreen;
        BuildMenu(); BuildDashboard();
        Controls.Add(_dash); Controls.Add(_menu); MainMenuStrip = _menu;
        Load += (s, e) => RefreshDashboard();
    }

    // Menu items carry a module tag; visibility comes from Session.Can
    void BuildMenu()
    {
        var file = TopMenu("File");
        Item(file, "Refresh dashboard", null, RefreshDashboard);
        Item(file, "Logout", null, () => { new AuthController().Logout(); LogoutRequested = true; Close(); });
        Item(file, "Exit", null, Close);

        var master = TopMenu("Master Data");
        Open(master, "Fuel Products", "Products", () => new ProductsForm());
        Open(master, "Tanks", "Tanks", () => new TanksForm());
        Open(master, "Pumps / Nozzles", "Pumps", () => new PumpsForm());
        Open(master, "Fuel Prices", "Prices", () => new PricesForm());
        Open(master, "Users", "Users", () => new UsersForm());

        var purch = TopMenu("Purchasing");
        Open(purch, "Suppliers", "Suppliers", () => new SuppliersForm());
        Open(purch, "Deliveries", "Deliveries", () => new DeliveriesForm());

        var ops = TopMenu("Operations");
        Open(ops, "Shifts & Pump Readings", "Shifts", () => new ShiftsForm());
        Open(ops, "Sales", "Sales", () => new SalesForm());

        var fin = TopMenu("Finance");
        Open(fin, "Customers", "Customers", () => new CustomersForm());
        Open(fin, "Payments", "Payments", () => new PaymentsForm());
        Open(fin, "Receive Credit Payment", "Payments", () => new ReceiveCreditForm());
        Open(fin, "Shift Reconciliation", "Reconciliation", () => new ReconciliationForm());
        Open(fin, "Expenses", "Expenses", () => new ExpensesForm());

        var rep = TopMenu("Reports");
        foreach (var r in ReportController.Reports) { var key = r.Key; Open(rep, r.Title, "Reports", () => new ReportForm(key)); }

        var adm = TopMenu("Administration");
        Open(adm, "Audit Log", "Audit", () => new AuditForm());
        Open(adm, "Void Transactions", "Voids", () => new VoidsForm());

        // show only what the role may use; hide empty top-level menus
        foreach (ToolStripMenuItem top in _menu.Items)
        {
            foreach (ToolStripItem sub in top.DropDownItems)
                if (sub.Tag is string module) sub.Available = Session.Can(module);
            if (top.Text != "File") top.Available = top.DropDownItems.Cast<ToolStripItem>().Any(i => i.Available);
        }
    }

    ToolStripMenuItem TopMenu(string text) { var m = new ToolStripMenuItem(text); _menu.Items.Add(m); return m; }

    // Menu item that opens a form as a dialog, then refreshes the dashboard
    void Open(ToolStripMenuItem parent, string text, string module, Func<Form> make) => Item(parent, text, module, () =>
    {
        using var f = make();
        f.ShowDialog(this);
        RefreshDashboard();
    });

    void Item(ToolStripMenuItem parent, string text, string? module, Action act)
    {
        var it = new ToolStripMenuItem(text) { Tag = module };
        it.Click += (s, e) => act();
        parent.DropDownItems.Add(it);
    }

    void BuildDashboard()
    {
        if (!Session.Can("Dashboard"))
        {
            _welcome.Text = $"Welcome, {Session.User!.FullName}.\nUse the menu to continue.";
            _dash.Controls.Add(_welcome);
            return;
        }
        var cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10, 10, 0, 0) };
        foreach (var (title, lbl) in new[] { ("Sales today", _sales), ("Deliveries today", _deliveries), ("Expenses today", _expenses), ("Outstanding credit", _credit) })
        {
            var box = new GroupBox { Text = title, Width = 240, Height = 84, Margin = new Padding(0, 0, 10, 0) };
            lbl.Dock = DockStyle.Fill; box.Controls.Add(lbl); cards.Controls.Add(box);
        }
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        var g1 = new GroupBox { Text = "Litres sold today by product", Dock = DockStyle.Fill }; g1.Controls.Add(_litres);
        var g2 = new GroupBox { Text = "Stock per tank (red = low)", Dock = DockStyle.Fill }; g2.Controls.Add(_tanks);
        split.Controls.Add(g1, 0, 0); split.Controls.Add(g2, 1, 0);
        _dash.Controls.Add(split); _dash.Controls.Add(cards);
        split.BringToFront();
        _tanks.DataBindingComplete += (s, e) => ColorLowTanks();
    }

    void ColorLowTanks()
    {
        foreach (DataGridViewRow row in _tanks.Rows)
            if (row.DataBoundItem is Tank k && k.IsLow)
            {
                row.DefaultCellStyle.BackColor = Color.MistyRose; row.DefaultCellStyle.ForeColor = Color.Firebrick;
                row.DefaultCellStyle.Font = new Font(Ui.AppFont, FontStyle.Bold);
            }
    }

    void RefreshDashboard()
    {
        if (!Session.Can("Dashboard")) return;
        var r = new ReportController().Dashboard();
        if (!Ui.Check(r)) return;
        var d = r.Value!;
        _sales.Text = d.SalesToday.ToString("N2"); _deliveries.Text = d.DeliveriesToday.ToString();
        _expenses.Text = d.ExpensesToday.ToString("N2"); _credit.Text = d.OutstandingCredit.ToString("N2");
        _litres.DataSource = d.LitresByProduct; Ui.Tidy(_litres);
        _tanks.DataSource = d.Tanks; Ui.Tidy(_tanks);
    }
}
