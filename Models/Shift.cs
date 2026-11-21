namespace PetrolStationMS.Models;
public class Shift
{
    public int Id { get; set; }
    public int AttendantId { get; set; }
    public string AttendantName { get; set; } = "";
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string Status { get; set; } = "Open";
    public override string ToString() => $"#{Id} {OpenedAt:yyyy-MM-dd HH:mm} - {AttendantName} ({Status})";
}
