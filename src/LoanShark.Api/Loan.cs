namespace LoanShark.Api;

public sealed record Loan(
    Guid Id,
    Guid UserId,
    decimal Amount,
    decimal Fee,
    decimal Balance,
    DateTimeOffset ApplicationDate,
    LoanStatus Status = LoanStatus.Pending,
    DateTimeOffset? ApprovalDate = null,
    Guid? ApprovedBy = null,
    DateTimeOffset? RepaymentDate = null)
{
    public static Loan Create(LoanApplicationReceived @event)
    {
        var (loanId, userId, amount, fee, applicationDate) = @event;
        var balance = amount + fee;
        return new(loanId, userId, amount, fee, balance, applicationDate);
    }

    public Loan Apply(LoanApplicationApproved @event) =>
        this with { Status = LoanStatus.Approved, ApprovalDate = DateTimeOffset.UtcNow, ApprovedBy = @event.ApprovedBy};

    public Loan Apply(LoanPaymentReceived @event) =>
        this with { Balance = Balance - @event.Amount };

    public Loan Apply(LoanRepaid @event) =>
        this with { Status = LoanStatus.Paid, RepaymentDate = DateTimeOffset.UtcNow };
}

public enum LoanStatus
{
    Pending,
    Approved,
    Paid
}