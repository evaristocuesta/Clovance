using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts;
using Clovance.ApiService.Features.Accounts.CreateAccount;

namespace Clovance.IntegrationTests.Features.Accounts;

public abstract class AccountIntegrationTestBase : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected AccountIntegrationTestBase(AspireFixture fixture) : base(fixture)
    {
    }

    protected static async Task<T?> ReadJsonAsync<T>(HttpContent content)
    {
        return await content.ReadFromJsonAsync<T>(JsonOptions, TestContext.Current.CancellationToken);
    }

    protected async Task<AccountDto> CreateAccountAsync(
        string? name = null,
        AccountType type = AccountType.Checking,
        string currency = "EUR",
        decimal openingBalance = 0m,
        DateOnly? openingDate = null,
        string openingDescription = "Opening balance")
    {
        var command = new CreateAccountCommand(
            Name: name ?? $"Account-{Guid.CreateVersion7()}",
            Type: type,
            Currency: currency,
            OpeningBalance: openingBalance,
            OpeningDate: openingDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            OpeningDescription: openingDescription);

        var response = await Client.PostAsJsonAsync("/api/accounts", command, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var account = await ReadJsonAsync<AccountDto>(response.Content);
        Assert.NotNull(account);

        return account!;
    }
}
