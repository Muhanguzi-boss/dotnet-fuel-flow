namespace PetrolStationMS.Models;
public class PumpReading
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public int PumpId { get; set; }
    public string PumpCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal OpeningReading { get; set; }
    public decimal? ClosingReading { get; set; }
    public decimal Litres => ClosingReading.HasValue ? ClosingReading.Value - OpeningReading : 0;
}
