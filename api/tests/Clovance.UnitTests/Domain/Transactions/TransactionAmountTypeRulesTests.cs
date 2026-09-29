using Clovance.ApiService.Domain.Transactions;

namespace Clovance.UnitTests.Domain.Transactions;

public class TransactionAmountTypeRulesTests
{
    [Fact]
    public void TransactionAmountTypeRules_ForIncome_AllowsPositiveValuesOnly()
    {
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(10m, 25m, TransactionType.Income));
        Assert.False(TransactionAmountTypeRules.EnsureAmountMatchesType(10m, -25m, TransactionType.Income));
    }

    [Fact]
    public void TransactionAmountTypeRules_ForExpense_AllowsNegativeValuesOnly()
    {
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(-10m, -25m, TransactionType.Expense));
        Assert.False(TransactionAmountTypeRules.EnsureAmountMatchesType(-10m, 25m, TransactionType.Expense));
    }

    [Fact]
    public void TransactionAmountTypeRules_ForTransfer_RequiresSameSignAndNonZero()
    {
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(100m, 25m, TransactionType.Transfer));
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(-100m, -25m, TransactionType.Transfer));
        Assert.False(TransactionAmountTypeRules.EnsureAmountMatchesType(100m, -25m, TransactionType.Transfer));
        Assert.False(TransactionAmountTypeRules.EnsureAmountMatchesType(100m, 0m, TransactionType.Transfer));
    }

    [Fact]
    public void TransactionAmountTypeRules_ForLoanPayment_AllowsAnyNonZeroValue()
    {
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(100m, 25m, TransactionType.LoanPayment));
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(-100m, -25m, TransactionType.LoanPayment));
        Assert.False(TransactionAmountTypeRules.EnsureAmountMatchesType(100m, 0m, TransactionType.LoanPayment));
    }

    [Fact]
    public void TransactionAmountTypeRules_ForOpeningBalance_AlwaysAllowsValue()
    {
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(0m, 0m, TransactionType.OpeningBalance));
        Assert.True(TransactionAmountTypeRules.EnsureAmountMatchesType(0m, -50m, TransactionType.OpeningBalance));
    }
}
