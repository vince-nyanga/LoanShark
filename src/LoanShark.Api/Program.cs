using System.Text.Json.Serialization;
using LoanShark.Api;
using Marten;
using Marten.AspNetCore;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Services.Json;
using Weasel.Core;
using static LoanShark.Api.LoanService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddMarten(provider =>
{
    var storeOptions = new StoreOptions();
    var schemaName = "LoanShark";
    storeOptions.Events.DatabaseSchemaName = schemaName;
    storeOptions.DatabaseSchemaName = schemaName;
    storeOptions.Connection(builder.Configuration.GetConnectionString("Default")!);
    
    storeOptions.UseDefaultSerialization(
        EnumStorage.AsString,
        nonPublicMembersStorage: NonPublicMembersStorage.All,
        serializerType: SerializerType.SystemTextJson);
    
    storeOptions.Projections.Add<LoanSummaryProjection>(ProjectionLifecycle.Async);
    
    return storeOptions;
})
.UseLightweightSessions()
.AddAsyncDaemon(DaemonMode.Solo);

var app = builder.Build();

app.MapGet("api/loans/{id:guid}", LoanApiHandler.GetLoan);
app.MapGet("api/loans/{id:guid}/summary", LoanApiHandler.GetLoanSummary);
app.MapPost("api/loans", LoanApiHandler.HandleNewApplication);
app.MapPost("api/loans/{id:guid}", LoanApiHandler.HandleLoanApproval);
app.MapPost("api/loans/{id:guid}/repay", LoanApiHandler.HandleLoanRepayment);

app.Run();

internal class LoanApiHandler
{

    public static async ValueTask<IResult> GetLoan(
        Guid id, 
        IDocumentSession documentSession)
    {
        var loan = await documentSession.Events.AggregateStreamAsync<Loan>(id);
        
        return loan is not null ? Results.Ok(loan) : Results.NotFound();
    }

    public static Task GetLoanSummary(Guid id, HttpContext context, IQuerySession querySession) =>
        querySession.Json.WriteById<LoanSummary>(id, context);
    
    public static async ValueTask<IResult> HandleNewApplication(
        IDocumentSession documentSession, 
        SubmitLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var loanId = Guid.NewGuid();
        var command = new SubmitLoanApplication(loanId, request.UserId, request.Amount);
        await documentSession.AddAsync(loanId, Handle(command), cancellationToken);
        return Results.Created($"/api/loans/{loanId}", loanId);
    }

    public static async ValueTask<IResult> HandleLoanApproval(
        Guid id, 
        IDocumentSession documentSession,
        ApproveLoanApplicationRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new ApproveLoanApplication(id, request.ApproverId);
        await documentSession.GetAndUpdateAsync(id, loan => Handle(loan, command), cancellationToken);

        return Results.Ok();
    }

    public static async ValueTask<IResult> HandleLoanRepayment(
        Guid id, 
        IDocumentSession documentSession,
        RepayLoanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RepayLoan(id, request.Amount);
        await documentSession.GetAndUpdateAsync(id, loan => Handle(loan, command), cancellationToken);

        return Results.Ok();
    }
}
