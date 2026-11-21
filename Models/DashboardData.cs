namespace PetrolStationMS.Models;
// Aggregated numbers shown on the main dashboard
public class DashboardData
{
    public decimal SalesToday { get; set; }
    public int DeliveriesToday { get; set; }
    public decimal ExpensesToday { get; set; }
    public decimal OutstandingCredit { get; set; }
    public DataTable LitresByProduct { get; set; } = new();
    public List<Tank> Tanks { get; set; } = new();
}
