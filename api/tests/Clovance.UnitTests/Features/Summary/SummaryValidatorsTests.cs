using Clovance.ApiService.Features.Summary.GetDailyBalance;
using Clovance.ApiService.Features.Summary.GetDailyCashflow;
using Clovance.ApiService.Features.Summary.GetMonthlyBalance;
using Clovance.ApiService.Features.Summary.GetMonthlyCashflow;
using Clovance.ApiService.Features.Summary.Shared;
using Clovance.ApiService.Shared;

namespace Clovance.UnitTests.Features.Summary;

public class SummaryValidatorsTests
{
    [Fact]
    public void GetDailyBalanceValidator_WhenMonthAndCurrencyAreInvalid_ReturnsErrors()
    {
        var validator = new GetDailyBalanceValidator();

        var result = validator.Validate(new GetDailyBalanceQuery(
            AccountId: null,
            Year: 2025,
            Month: 0,
            Currency: string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.Transactions.MonthInvalidRange);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountCurrencyInvalid);
    }

    [Fact]
    public void SummaryValidators_WhenAccountIdAndAccountTypeAreBothProvided_ReturnValidationErrors()
    {
        var dailyBalance = new GetDailyBalanceValidator().Validate(new GetDailyBalanceQuery(
            AccountId: Guid.NewGuid(),
            Year: 2025,
            Month: 1,
            Currency: "EUR",
            AccountType: AccountTypeFilter.Asset));

        var dailyCashflow = new GetDailyCashflowValidator().Validate(new GetDailyCashflowQuery(
            AccountId: Guid.NewGuid(),
            Year: 2025,
            Month: 1,
            Currency: "EUR",
            AccountType: AccountTypeFilter.Asset));

        var monthlyBalance = new GetMonthlyBalanceValidator().Validate(new GetMonthlyBalanceQuery(
            AccountId: Guid.NewGuid(),
            Currency: "EUR",
            MonthsBack: 2,
            AnchorYear: 2025,
            AnchorMonth: 1,
            AccountType: AccountTypeFilter.Asset));

        var monthlyCashflow = new GetMonthlyCashflowValidator().Validate(new GetMonthlyCashflowQuery(
            AccountId: Guid.NewGuid(),
            Currency: "EUR",
            MonthsBack: 2,
            AnchorYear: 2025,
            AnchorMonth: 1,
            AccountType: AccountTypeFilter.Asset));

        Assert.False(dailyBalance.IsValid);
        Assert.False(dailyCashflow.IsValid);
        Assert.False(monthlyBalance.IsValid);
        Assert.False(monthlyCashflow.IsValid);

        Assert.Contains(dailyBalance.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
        Assert.Contains(dailyCashflow.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
        Assert.Contains(monthlyBalance.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
        Assert.Contains(monthlyCashflow.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
    }

    [Fact]
    public void SummaryValidators_WhenAccountTypeIsInvalid_ReturnValidationErrors()
    {
        var invalidAccountType = (AccountTypeFilter)999;

        var dailyBalance = new GetDailyBalanceValidator().Validate(new GetDailyBalanceQuery(
            AccountId: null,
            Year: 2025,
            Month: 1,
            Currency: "EUR",
            AccountType: invalidAccountType));

        var dailyCashflow = new GetDailyCashflowValidator().Validate(new GetDailyCashflowQuery(
            AccountId: null,
            Year: 2025,
            Month: 1,
            Currency: "EUR",
            AccountType: invalidAccountType));

        var monthlyBalance = new GetMonthlyBalanceValidator().Validate(new GetMonthlyBalanceQuery(
            AccountId: null,
            Currency: "EUR",
            MonthsBack: 2,
            AnchorYear: 2025,
            AnchorMonth: 1,
            AccountType: invalidAccountType));

        var monthlyCashflow = new GetMonthlyCashflowValidator().Validate(new GetMonthlyCashflowQuery(
            AccountId: null,
            Currency: "EUR",
            MonthsBack: 2,
            AnchorYear: 2025,
            AnchorMonth: 1,
            AccountType: invalidAccountType));

        Assert.False(dailyBalance.IsValid);
        Assert.False(dailyCashflow.IsValid);
        Assert.False(monthlyBalance.IsValid);
        Assert.False(monthlyCashflow.IsValid);

        Assert.Contains(dailyBalance.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
        Assert.Contains(dailyCashflow.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
        Assert.Contains(monthlyBalance.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
        Assert.Contains(monthlyCashflow.Errors, e => e.ErrorCode == ErrorCodes.Accounts.AccountTypeInvalid);
    }
}
