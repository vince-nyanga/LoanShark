namespace LoanShark.Api;

public interface ILoanEvent
{
    Guid Id { get; init; }
}

public sealed record LoanApplicationReceived(
    Guid Id,
    Guid UserId,
    decimal Amount,
    decimal Fee,
    DateTimeOffset ApplicationDate) : ILoanEvent;

public sealed record LoanApplicationApproved(
    Guid Id,
    Guid ApprovedBy,
    DateTimeOffset DateApproved) : ILoanEvent;

public sealed record LoanPaymentReceived(
    Guid Id,
    decimal Amount,
    DateTimeOffset PaymentDate) : ILoanEvent;


public sealed record LoanRepaid(
    Guid Id,
    DateTimeOffset DateRepaid) : ILoanEvent;