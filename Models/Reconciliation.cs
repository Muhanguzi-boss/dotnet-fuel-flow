namespace PetrolStationMS.Models;
public class Reconciliation
{
    public int ShiftId { get; set; }
    public decimal ExpectedSales { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal Difference => TotalPayments - ExpectedSales;
    public string Status => Difference == 0 ? "Balanced" : Difference < 0 ? "Shortage" : "Overage";
    public DataTable ByMethod { get; set; } = new();
}
