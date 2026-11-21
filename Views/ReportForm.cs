namespace PetrolStationMS.Views;
// One screen for all reports: pick the report and a date range
public class ReportForm : DateFilterForm
{
    readonly ReportController _c = new();
    readonly ComboBox _report = Ui.Combo();

    public ReportForm(string key) : base("Reports", 980, 560)
    {
        _report.Width = 230;
        foreach (var r in ReportController.Reports) _report.Items.Add(r.Title);
        _report.SelectedIndex = Math.Max(0, Array.FindIndex(ReportController.Reports, r => r.Key == key));
        Bar.Controls.Add(new Label { Text = "Report", AutoSize = true, Margin = new Padding(16, 6, 4, 0) });
        Bar.Controls.Add(_report);
        _report.SelectedIndexChanged += (s, e) => Reload();
    }

    protected override void Reload()
    {
        if (_report.SelectedIndex < 0) return;
        var rep = ReportController.Reports[_report.SelectedIndex];
        From.Enabled = To.Enabled = rep.UsesDates;
        Text = rep.Title;
        Ui.Bind(Grid, _c.Build(rep.Key, From.Value, To.Value));
    }
}
