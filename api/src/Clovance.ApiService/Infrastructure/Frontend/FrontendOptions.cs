namespace Clovance.ApiService.Infrastructure.Frontend;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    public required string BaseUrl { get; init; }
}
