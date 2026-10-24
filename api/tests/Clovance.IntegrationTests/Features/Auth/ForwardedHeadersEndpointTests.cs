using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Clovance.ApiService.Features.Auth.Login;

namespace Clovance.IntegrationTests.Features.Auth;

public class ForwardedHeadersEndpointTests : IntegrationTestBase
{
    public ForwardedHeadersEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task LoginEndpoint_SetsSecureCookie_WhenForwardedProtoIsHttps()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, email, password) = await CreateTestUserAsync();

        var response = await PostLoginAsync(email, password, forwardedProto: "https", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(HasSecureFlag(GetSetCookieHeader(response)));
    }

    [Fact]
    public async Task LoginEndpoint_DoesNotSetSecureCookie_WithoutForwardedProtoHeader()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, email, password) = await CreateTestUserAsync();

        var response = await PostLoginAsync(email, password, forwardedProto: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(HasSecureFlag(GetSetCookieHeader(response)));
    }

    private async Task<HttpResponseMessage> PostLoginAsync(
        string email,
        string password,
        string? forwardedProto,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginCommand(Email: email, Password: password))
        };

        if (forwardedProto is not null)
        {
            request.Headers.Add("X-Forwarded-Proto", forwardedProto);
        }

        return await Client.SendAsync(request, ct);
    }

    private static string GetSetCookieHeader(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var values));
        return string.Join(" ", values!);
    }

    private static bool HasSecureFlag(string setCookieHeader) =>
        Regex.IsMatch(setCookieHeader, @";\s*secure(\s*;|$)", RegexOptions.IgnoreCase);
}
