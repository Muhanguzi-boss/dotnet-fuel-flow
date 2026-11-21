namespace PetrolStationMS.Models;
public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "L";
    public bool IsActive { get; set; } = true;
    public override string ToString() => Name;
}
