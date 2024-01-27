using Marten;

namespace LoanShark.Api;

internal static class DocumentSessionExtensions
{
    public static async Task AddAsync(
        this IDocumentSession documentSession,
        Guid id,
        IEnumerable<ILoanEvent> events,
        CancellationToken cancellationToken)
    {
        documentSession.Events.StartStream<Loan>(id, events);
        await documentSession.SaveChangesAsync(cancellationToken);
    }

    public static async Task GetAndUpdateAsync(
        this IDocumentSession documentSession,
        Guid id,
        Func<Loan, IEnumerable<ILoanEvent>> handle,
        CancellationToken cancellationToken)
    {
        var loan = await documentSession.Events.AggregateStreamAsync<Loan>(id, token: cancellationToken);

        if (loan is null)
            throw new InvalidOperationException($"Loan with id {id} does not exist");

        var events = handle(loan);

        await documentSession.Events.WriteToAggregate<Loan>(id, stream => stream.AppendMany(events), cancellationToken);
    }
}