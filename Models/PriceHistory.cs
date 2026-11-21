namespace PetrolStationMS.Models;
public class PriceHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public int SetBy { get; set; }
    public string SetByName { get; set; } = "";
}
