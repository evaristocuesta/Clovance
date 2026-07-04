using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.GetCurrencies;

public class GetCurrenciesQueryHandler : IHandler<GetCurrenciesQuery, GetCurrenciesResult>
{
    public Task<GetCurrenciesResult> HandleAsync(GetCurrenciesQuery command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GetCurrenciesResult(Currencies.Values.Values));
    }
}
