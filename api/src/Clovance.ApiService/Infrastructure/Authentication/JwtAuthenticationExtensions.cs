using System.Diagnostics;
using System.Text;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Clovance.ApiService.Infrastructure.Authentication;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("AuthEvents");

                        logger.LogWarning(
                            context.Exception,
                            "JWT authentication failed. Path: {Path}, TraceId: {TraceId}",
                            context.Request.Path,
                            context.HttpContext.TraceIdentifier);

                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.HandleResponse();

                        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                        var status = StatusCodes.Status401Unauthorized;

                        var problemDetails = new ProblemDetails
                        {
                            Status = status,
                            Title = "Unauthorized",
                            Detail = "Authentication is required or the bearer token is invalid.",
                            Instance = context.Request.Path,
                            Type = $"https://httpstatuses.com/{status}",
                            Extensions =
                            {
                                ["traceId"] = traceId,
                                ["errorCode"] = ErrorCodes.Common.Unauthorized
                            }
                        };

                        context.Response.StatusCode = status;
                        await context.Response.WriteAsJsonAsync(problemDetails);
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                        var status = StatusCodes.Status403Forbidden;

                        var problemDetails = new ProblemDetails
                        {
                            Status = status,
                            Title = "Forbidden",
                            Detail = "You do not have permission to access this resource.",
                            Instance = context.Request.Path,
                            Type = $"https://httpstatuses.com/{status}",
                            Extensions =
                            {
                                ["traceId"] = traceId,
                                ["errorCode"] = ErrorCodes.Common.Forbidden
                            }
                        };

                        context.Response.StatusCode = status;
                        await context.Response.WriteAsJsonAsync(problemDetails);
                    }
                };
            });

        return services;
    }
}
