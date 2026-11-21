namespace PetrolStationMS.Models;
public class Delivery
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int TankId { get; set; }
    public string TankCode { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? InvoiceRef { get; set; }
    public int ReceivedBy { get; set; }
    public string ReceivedByName { get; set; } = "";
    public DateTime ReceivedAt { get; set; }
    public bool IsVoid { get; set; }
    public string? VoidReason { get; set; }
    public int? VoidedBy { get; set; }
    public DateTime? VoidedAt { get; set; }
}
