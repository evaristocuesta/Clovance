using Microsoft.AspNetCore.Http;

namespace Clovance.ApiService.Infrastructure.Authentication;

public static class RefreshTokenCookieExtensions
{
    private const string RefreshTokenCookieName = "clovance-refresh-token";

    /// <summary>
    /// Sets the refresh token cookie with secure configuration.
    /// </summary>
    /// <param name="response">The HTTP response object</param>
    /// <param name="refreshToken">The refresh token value</param>
    /// <param name="expiresAt">When the cookie should expire</param>
    public static void SetRefreshTokenCookie(
        this Microsoft.AspNetCore.Http.HttpResponse response,
        string refreshToken,
        DateTimeOffset expiresAt)
    {
        var httpContext = response.HttpContext;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/api/auth/refresh"
        };

        response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    /// <summary>
    /// Gets the refresh token from the request cookies.
    /// </summary>
    /// <param name="request">The HTTP request object</param>
    /// <returns>The refresh token value, or null if not present</returns>
    public static string? GetRefreshTokenCookie(this Microsoft.AspNetCore.Http.HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookieName];
    }

    /// <summary>
    /// Removes the refresh token cookie.
    /// </summary>
    /// <param name="response">The HTTP response object</param>
    public static void DeleteRefreshTokenCookie(this Microsoft.AspNetCore.Http.HttpResponse response)
    {
        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/api/auth/refresh"
        });
    }
}
