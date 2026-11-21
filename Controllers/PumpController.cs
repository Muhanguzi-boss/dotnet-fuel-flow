namespace PetrolStationMS.Controllers;

public class PumpController : BaseController
{
    public static readonly string[] Statuses = { "Active", "Inactive", "Maintenance" };
    readonly PumpRepository _repo = new();
    public Result<List<Pump>> GetAll(bool activeOnly = false) => Run(() => _repo.GetAll(activeOnly));

    public Result Save(Pump p) => Run(() =>
    {
        Need("Pumps");
        p.Code = Clean(p.Code).ToUpper();
        Require(p.Code != "", "Pump/nozzle code is required.");
        Require(p.TankId > 0, "Select a tank.");
        Require(Statuses.Contains(p.Status), "Select a valid status.");
        if (p.Id == 0) { _repo.Insert(p); AuditController.Write("PUMP_ADD", $"Added pump {p.Code}"); }
        else { _repo.Update(p); AuditController.Write("PUMP_EDIT", $"Edited pump {p.Code} (status {p.Status})"); }
        return Result.Success();
    });
}
