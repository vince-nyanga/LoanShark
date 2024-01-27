namespace LoanShark.Api;

internal static class LoanService
{
    public static IEnumerable<ILoanEvent> Handle(SubmitLoanApplication command)
    {
        var (loanId, userId, amount) = command;
        yield return new LoanApplicationReceived(
            loanId, 
            userId, 
            amount, 
            amount * 0.1m, 
            DateTimeOffset.UtcNow);
    }

    public static IEnumerable<ILoanEvent> Handle(Loan loan, ApproveLoanApplication command)
    {
        if (loan.Status != LoanStatus.Pending)
            throw new InvalidOperationException("Cannot approve a loan that is not pending.");

        var (loanId, approvedBy) = command;
        yield return new LoanApplicationApproved(loanId, approvedBy, DateTimeOffset.UtcNow);
    }

    public static IEnumerable<ILoanEvent> Handle(Loan loan, RepayLoan command)
    {
        if (loan.Status != LoanStatus.Approved)
            throw new InvalidOperationException("Invalid status");

        if (command.Amount > loan.Balance)
            throw new InvalidOperationException("Please don't overpay us!");

        var (loanId, amount) = command;
        var now = DateTimeOffset.UtcNow;

        yield return new LoanPaymentReceived(loanId, amount, now);

        if (command.Amount == loan.Balance)
            yield return new LoanRepaid(loanId, now);
    }
}