namespace PetrolStationMS.Models;
public class Tank
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Capacity { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal LowThreshold { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsLow => CurrentStock <= LowThreshold;
    public override string ToString() => $"{Code} ({ProductName})";
}
