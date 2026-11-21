namespace PetrolStationMS.Data;
public class ExpenseRepository : Repo
{
    public ExpenseRepository(IDbConnection? c = null, IDbTransaction? t = null) : base(c, t) { }
    public List<Expense> GetRange(DateTime from, DateTime to) => Run((c, t) => c.Query<Expense>(
        @"SELECT e.id, e.category, e.amount, e.payee, e.description, e.expense_date::timestamp AS expense_date,
          e.recorded_by, e.created_at, u.full_name AS recorded_by_name FROM expenses e JOIN users u ON u.id = e.recorded_by
          WHERE e.expense_date >= @from AND e.expense_date <= @to ORDER BY e.expense_date DESC, e.id DESC",
        new { from = from.Date, to = to.Date }, t).ToList());
    public int Insert(Expense e) => Run((c, t) => c.ExecuteScalar<int>(
        @"INSERT INTO expenses(category, amount, payee, description, expense_date, recorded_by)
          VALUES (@Category, @Amount, @Payee, @Description, @ExpenseDate, @RecordedBy) RETURNING id", e, t));
}
