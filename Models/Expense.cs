namespace PetrolStationMS.Models;
public class Expense
{
    public int Id { get; set; }
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string Payee { get; set; } = "";
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int RecordedBy { get; set; }
    public string RecordedByName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
