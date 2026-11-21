namespace PetrolStationMS.Views;

public abstract class DateFilterForm : BaseForm
{
    protected readonly DataGridView Grid = Ui.Grid();
    protected readonly DateTimePicker From = Ui.DatePick(DateTime.Today.AddDays(-30)), To = Ui.DatePick();
    protected readonly FlowLayoutPanel Bar = new() { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 8, 0, 0) };

    protected DateFilterForm(string title, int w = 980, int h = 560) : base(title, w, h)
    {
        From.Width = 110; To.Width = 110;
        Bar.Controls.Add(new Label { Text = "From", AutoSize = true, Margin = new Padding(0, 6, 4, 0) });
        Bar.Controls.Add(From);
        Bar.Controls.Add(new Label { Text = "To", AutoSize = true, Margin = new Padding(8, 6, 4, 0) });
        Bar.Controls.Add(To);
        Bar.Controls.Add(Ui.Btn("Refresh", (s, e) => Reload(), 80));
        Controls.Add(Grid); Controls.Add(Bar); Grid.BringToFront();
        Load += (s, e) => Reload();
    }
    protected abstract void Reload();
}
