using System.Security.Claims;
using Clovance.ApiService.Features.Auth;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add HttpContextAccessor for handlers
builder.Services.AddHttpContextAccessor();

// Register handlers
builder.Services.AddHandlersFromAssembly(typeof(Program).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapGet("/", () => Results.Redirect("/scalar", permanent: false));

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClovanceDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Map Auth endpoints
app.MapAuthEndpoints();

app.Use(async (context, next) =>
{
    if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        await next();
        return;
    }

    var endpoint = context.GetEndpoint();
    var path = context.Request.Path;

    if (path.StartsWithSegments("/auth/complete-onboarding", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null)
    {
        await next();
        return;
    }

    var mustChangePasswordClaim = context.User.FindFirstValue("must_complete_onboarding");

    var mustChangePassword = string.Equals(mustChangePasswordClaim, bool.TrueString, StringComparison.OrdinalIgnoreCase);

    var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
    var user = await userManager.GetUserAsync(context.User);
    if (user is not null)
    {
        mustChangePassword = user.MustCompleteOnboarding;
    }

    if (mustChangePassword)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "password_change_required",
            detail = "You must change your password before accessing protected endpoints."
        });
        return;
    }

    await next();
});

app.MapDefaultEndpoints();

await app.SeedIdentityAsync();

app.Run();

