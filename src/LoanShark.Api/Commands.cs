namespace LoanShark.Api;

public sealed record SubmitLoanApplication(
    Guid LoanId,
    Guid UserId,
    decimal Amount);

public sealed record ApproveLoanApplication(
    Guid LoanId,
    Guid ApprovedBy);

public sealed record RepayLoan(
    Guid LoanId,
    decimal Amount);