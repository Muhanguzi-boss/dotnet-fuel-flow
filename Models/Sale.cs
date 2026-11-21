namespace PetrolStationMS.Models;
public class Sale
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public int PumpId { get; set; }
    public string PumpCode { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string AttendantName { get; set; } = "";
    public decimal Litres { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVoid { get; set; }
    public string? VoidReason { get; set; }
    public int? VoidedBy { get; set; }
    public DateTime? VoidedAt { get; set; }
    public override string ToString() => $"#{Id}  {PumpCode}  {Litres:N2} L = {Total:N2}";
}
