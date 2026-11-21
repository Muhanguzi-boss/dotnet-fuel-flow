namespace PetrolStationMS.Views;
public class ExpensesForm : DateFilterForm
{
    readonly ExpenseController _c = new();
    readonly ComboBox _cat = Ui.Combo();
    readonly NumericUpDown _amount = Ui.Num();
    readonly TextBox _payee = new() { MaxLength = 100 }, _desc = new() { MaxLength = 300 };
    readonly DateTimePicker _date = Ui.DatePick();

    public ExpensesForm() : base("Expenses", 1000, 600)
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 8, 8, 0) };
        _cat.Items.AddRange(ExpenseController.Categories);
        Ui.Field(top, "Category", _cat, 130); Ui.Field(top, "Amount", _amount, 110); Ui.Field(top, "Payee", _payee, 150);
        Ui.Field(top, "Description", _desc, 200); Ui.Field(top, "Date", _date, 110);
        var btn = Ui.Btn("Add Expense", (s, e) => Add(), 110); btn.Margin = new Padding(0, 20, 0, 0);
        top.Controls.Add(btn);
        Controls.Add(top);
    }

    protected override void Reload() => Ui.Bind(Grid, _c.GetRange(From.Value, To.Value));

    void Add()
    {
        var e = new Expense { Category = _cat.SelectedItem as string ?? "", Amount = _amount.Value, Payee = _payee.Text, Description = _desc.Text, ExpenseDate = _date.Value.Date };
        if (!Ui.Check(_c.Add(e))) return;
        _amount.Value = 0; _payee.Text = ""; _desc.Text = "";
        Reload();
    }
}
