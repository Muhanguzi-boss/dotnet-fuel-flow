namespace PetrolStationMS.Controllers;

public class PriceController : BaseController
{
    readonly PriceRepository _repo = new();
    public Result<List<PriceHistory>> GetHistory() => Run(() => _repo.GetHistory());
    public Result<decimal?> Current(int productId) => Run(() => _repo.GetPriceAt(productId, DateTime.Now));

    public Result SetPrice(int productId, decimal price) => Run(() =>
    {
        Need("Prices");
        Require(productId > 0, "Select a product.");
        Require(price > 0 && price <= 1_000_000, "Price must be greater than 0.");
        var product = new ProductRepository().Get(productId) ?? throw new BusinessException("Product not found.");
        Db.InTransaction((c, t) =>
        {
            var old = new PriceRepository(c, t).GetPriceAt(productId, DateTime.Now);
            new PriceRepository(c, t).Insert(productId, price, Session.UserId);
            AuditController.Write("PRICE_CHANGE", $"{product.Name}: {old?.ToString("N2") ?? "none"} -> {price:N2}", c, t);
            return 0;
        });
        return Result.Success();
    });
}
