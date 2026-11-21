namespace PetrolStationMS.Controllers;

public class PaymentController : BaseController
{
    public static readonly string[] Methods = { "Cash", "Mobile Money", "Card", "Credit" };
    public static readonly string[] ReceiptMethods = { "Cash", "Mobile Money", "Card" };

    public Result<List<Payment>> GetByShift(int shiftId) => Run(() => new PaymentRepository().GetByShift(shiftId));
    public Result<List<Payment>> GetRange(DateTime from, DateTime to) => Run(() => new PaymentRepository().GetRange(from, to));
    public Result<List<Sale>> SalesOfShift(int shiftId) => Run(() => new SaleRepository().GetByShift(shiftId));

    public Result Record(Payment p) => Run(() =>
    {
        Need("Payments");
        p.Reference = Clean(p.Reference);
        Require(p.ShiftId > 0, "Select a shift.");
        Require(Methods.Contains(p.Method), "Select a payment method.");
        Require(p.Amount > 0, "Amount must be greater than 0.");
        if (p.Method == "Credit") Require(p.CustomerId > 0, "Select the customer for a credit sale.");
        p.Kind = "Sale"; p.RecordedBy = Session.UserId;
        Db.InTransaction((c, t) =>
        {
            if (p.SaleId.HasValue)
            {
                var sale = new SaleRepository(c, t).Get(p.SaleId.Value) ?? throw new BusinessException("Sale not found.");
                Require(!sale.IsVoid, "That sale is void.");
                Require(sale.ShiftId == p.ShiftId, "That sale belongs to a different shift.");
            }
            if (p.Method == "Credit")
            {
                var cust = new CustomerRepository(c, t).Get(p.CustomerId!.Value, true) ?? throw new BusinessException("Customer not found.");
                Require(cust.IsActive, "That customer is inactive.");
                if (cust.Balance + p.Amount > cust.CreditLimit)
                    throw new BusinessException($"Credit limit exceeded for {cust.Name}. Limit {cust.CreditLimit:N2}, balance {cust.Balance:N2}, available {cust.CreditLimit - cust.Balance:N2}.");
                new CustomerRepository(c, t).AdjustBalance(cust.Id, p.Amount);
                AuditController.Write("CREDIT_SALE", $"Credit {p.Amount:N2} to {cust.Name} (shift #{p.ShiftId})", c, t);
            }
            else p.CustomerId = null;
            new PaymentRepository(c, t).Insert(p);
            return 0;
        });
        return Result.Success();
    });

    public Result ReceiveCredit(int customerId, string method, decimal amount, string? reference) => Run(() =>
    {
        Need("Payments");
        Require(customerId > 0, "Select a customer.");
        Require(ReceiptMethods.Contains(method), "Select a payment method.");
        Require(amount > 0, "Amount must be greater than 0.");
        Db.InTransaction((c, t) =>
        {
            var cust = new CustomerRepository(c, t).Get(customerId, true) ?? throw new BusinessException("Customer not found.");
            if (amount > cust.Balance) throw new BusinessException($"Amount exceeds the outstanding balance ({cust.Balance:N2}).");
            new CustomerRepository(c, t).AdjustBalance(customerId, -amount);
            new PaymentRepository(c, t).Insert(new Payment
            {
                CustomerId = customerId, Kind = "CreditReceipt", Method = method, Amount = amount,
                Reference = Clean(reference), RecordedBy = Session.UserId
            });
            AuditController.Write("CREDIT_PAYMENT", $"Received {amount:N2} ({method}) from {cust.Name}", c, t);
            return 0;
        });
        return Result.Success();
    });

    public Result<Reconciliation> Reconcile(int shiftId) => Run(() =>
    {
        Need("Reconciliation");
        var repo = new PaymentRepository();
        var bm = repo.ByMethod(shiftId);
        decimal paid = 0;
        foreach (DataRow r in bm.Rows) paid += Convert.ToDecimal(r["Amount"]);
        return new Reconciliation { ShiftId = shiftId, ExpectedSales = repo.ExpectedSales(shiftId), TotalPayments = paid, ByMethod = bm };
    });

    public Result Void(int id, string reason) => Run(() =>
    {
        Need("Voids");
        reason = Clean(reason);
        Require(reason.Length >= 3, "A void reason is required.");
        Db.InTransaction((c, t) =>
        {
            var repo = new PaymentRepository(c, t);
            var p = repo.Get(id, true) ?? throw new BusinessException("Payment not found.");
            Require(!p.IsVoid, "This payment is already void.");
            if (p.CustomerId.HasValue)
            {
                var cust = new CustomerRepository(c, t);
                var delta = p.Kind == "CreditReceipt" ? p.Amount : (p.Method == "Credit" ? -p.Amount : 0);
                if (delta != 0 && !cust.AdjustBalance(p.CustomerId.Value, delta))
                    throw new BusinessException("Cannot void: the customer's balance is lower than this credit (part of it was already repaid).");
            }
            repo.MarkVoid(id, reason, Session.UserId);
            AuditController.Write("VOID_PAYMENT", $"Voided payment #{id} ({p.Method} {p.Amount:N2}). Reason: {reason}", c, t);
            return 0;
        });
        return Result.Success();
    });
}
