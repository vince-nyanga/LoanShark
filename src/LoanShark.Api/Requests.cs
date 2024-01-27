namespace LoanShark.Api;

public sealed record SubmitLoanApplicationRequest(
    Guid UserId,
    decimal Amount);

public sealed record ApproveLoanApplicationRequest(
    Guid ApproverId);
    
    public sealed record RepayLoanRequest(decimal Amount);