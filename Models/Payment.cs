namespace PetrolStationMS.Models;
public class Payment
{
    public int Id { get; set; }
    public int? ShiftId { get; set; }
    public int? SaleId { get; set; }
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Kind { get; set; } = "Sale";
    public string Method { get; set; } = "Cash";
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public DateTime PaidAt { get; set; }
    public int RecordedBy { get; set; }
    public string RecordedByName { get; set; } = "";
    public bool IsVoid { get; set; }
    public string? VoidReason { get; set; }
    public int? VoidedBy { get; set; }
    public DateTime? VoidedAt { get; set; }
}
