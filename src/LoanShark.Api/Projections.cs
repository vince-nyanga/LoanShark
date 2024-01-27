using Marten.Events.Aggregation;

namespace LoanShark.Api;

public sealed record LoanSummary(
    Guid Id,
    Guid UserId,
    decimal AmountToPayBack,
    decimal RemainingBalance,
    ushort NumberOfRepayments,
    bool IsApproved);

public sealed class LoanSummaryProjection : SingleStreamProjection<LoanSummary>
{
    public LoanSummary Create(LoanApplicationReceived @event) =>
        new(
            @event.Id,
            @event.UserId,
            @event.Amount + @event.Fee,
            @event.Amount + @event.Fee,
            0,
            false);
    
    public LoanSummary Apply(LoanApplicationApproved @event, LoanSummary summary) =>
        summary with { IsApproved = true };

    public LoanSummary Apply(LoanPaymentReceived @event, LoanSummary summary) =>
        summary with
        {
            NumberOfRepayments = (ushort)(summary.NumberOfRepayments + 1),
            RemainingBalance = summary.RemainingBalance - @event.Amount
        };
}