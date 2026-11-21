namespace PetrolStationMS.Models;
public class Pump
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public int TankId { get; set; }
    public string TankCode { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string Status { get; set; } = "Active";
    public override string ToString() => Code;
}
