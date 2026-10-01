using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Clovance.UnitTests.Features.Transactions;

public abstract class TransactionHandlerTestBase : FeatureTestBase
{
    protected ClovanceDbContext Context { get; }
    protected IHttpContextAccessor HttpContextAccessor { get; }

    protected TransactionHandlerTestBase()
    {
        Context = TestDbContextFactory.CreateInMemoryDbContext();
        HttpContextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    protected void Authenticate(Guid userId)
    {
        FeatureTestBase.Authenticate(HttpContextAccessor, userId);
    }

    protected static Account CreateAccount(string name, AccountType type, string currency, Guid? createdBy = null)
    {
        return Account.Create(name, type, currency, createdBy ?? Guid.CreateVersion7());
    }

    protected static Transfer CreateTransferPair(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount = 200m,
        string description = "Transfer",
        DateOnly? date = null,
        Guid? createdBy = null)
    {
        var transfer = Transaction.CreateTransfer(
            amount,
            description,
            fromAccountId,
            toAccountId,
            date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            createdBy ?? Guid.CreateVersion7());

        transfer.From.ChangeRelatedTransactionId(transfer.To.Id);
        transfer.To.ChangeRelatedTransactionId(transfer.From.Id);

        return transfer;
    }

    protected static LoanPayment CreateLoanPaymentPair(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount = 500m,
        decimal principalAmount = 250m,
        string description = "Loan payment",
        DateOnly? date = null,
        Guid? createdBy = null)
    {
        var payment = Transaction.CreateLoanPayment(
            amount,
            principalAmount,
            description,
            fromAccountId,
            toAccountId,
            date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            createdBy ?? Guid.CreateVersion7());

        payment.From.ChangeRelatedTransactionId(payment.To.Id);
        payment.To.ChangeRelatedTransactionId(payment.From.Id);

        return payment;
    }
}
