using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClovanceDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapGet("/", () => "API service is running.");

var authGroup = app.MapGroup("/auth");

authGroup.MapPost("/login", async (
    [FromBody] LoginRequest request,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
    
    if (!signInResult.Succeeded)
    {
        return Results.Unauthorized();
    }

    var tokenResult = await signInManager.CreateUserPrincipalAsync(user);
    
    if (tokenResult.Identity is ClaimsIdentity identity)
    {
        identity.AddClaim(new Claim("must_complete_onboarding", user.MustCompleteOnboarding.ToString()));
    }

    return TypedResults.SignIn(tokenResult, authenticationScheme: IdentityConstants.BearerScheme);
});

authGroup.MapPost("/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(IdentityConstants.BearerScheme);
    return Results.NoContent();
}).RequireAuthorization();

authGroup.MapPost("/invitations", async (
    [FromBody] CreateInvitationRequest request,
    UserManager<ApplicationUser> userManager,
    ClovanceDbContext dbContext,
    IInvitationTokenService tokenService,
    IOptions<InvitationOptions> invitationOptions,
    HttpContext httpContext) =>
{
    if (!new EmailAddressAttribute().IsValid(request.Email))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Email"] = ["A valid email address is required."]
        });
    }

    var adminUserId = userManager.GetUserId(httpContext.User);
    if (string.IsNullOrWhiteSpace(adminUserId))
    {
        return Results.Unauthorized();
    }

    var normalizedEmail = request.Email.Trim();

    var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
    if (existingUser is not null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Email"] = ["A user with this email already exists."]
        });
    }

    var activeInvitation = dbContext.UserInvitations
        .FirstOrDefault(i => i.Email == normalizedEmail && i.ConsumedAt == null && i.ExpiresAt > DateTimeOffset.UtcNow);

    if (activeInvitation is not null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Email"] = ["There is already an active invitation for this email."]
        });
    }

    var rawToken = tokenService.GenerateToken();
    var tokenHash = tokenService.HashToken(rawToken);
    var expiresAt = DateTimeOffset.UtcNow.AddHours(Math.Max(1, invitationOptions.Value.ExpirationHours));

    var invitation = new UserInvitation
    {
        Id = Guid.NewGuid(),
        Email = normalizedEmail,
        TokenHash = tokenHash,
        ExpiresAt = expiresAt,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedByUserId = adminUserId
    };

    dbContext.UserInvitations.Add(invitation);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/auth/invitations/{invitation.Id}", new
    {
        invitation.Id,
        invitation.Email,
        invitation.ExpiresAt,
        Token = rawToken
    });
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

authGroup.MapPost("/register-with-invitation", async (
    [FromBody] RegisterWithInvitationRequest request,
    UserManager<ApplicationUser> userManager,
    ClovanceDbContext dbContext,
    IInvitationTokenService tokenService) =>
{
    if (!new EmailAddressAttribute().IsValid(request.Email))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Email"] = ["A valid email address is required."]
        });
    }

    if (string.IsNullOrWhiteSpace(request.Token))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Token"] = ["Invitation token is required."]
        });
    }

    var normalizedEmail = request.Email.Trim();
    var tokenHash = tokenService.HashToken(request.Token.Trim());

    var invitation = dbContext.UserInvitations
        .FirstOrDefault(i => i.Email == normalizedEmail && i.TokenHash == tokenHash);

    if (invitation is null || invitation.ConsumedAt is not null || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
    {
        return Results.Unauthorized();
    }

    var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
    if (existingUser is not null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Email"] = ["A user with this email already exists."]
        });
    }

    var user = new ApplicationUser
    {
        UserName = normalizedEmail,
        Email = normalizedEmail,
        EmailConfirmed = true,
        MustCompleteOnboarding = false
    };

    var createResult = await userManager.CreateAsync(user, request.Password);
    if (!createResult.Succeeded)
    {
        return Results.ValidationProblem(createResult.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
    }

    invitation.ConsumedAt = DateTimeOffset.UtcNow;
    invitation.ConsumedByUserId = user.Id;
    await dbContext.SaveChangesAsync();

    return Results.Created($"/auth/users/{user.Id}", new { user.Id, user.Email });
});

authGroup.MapPost("/complete-onboarding", async (
    [FromBody] CompleteOnboardingRequest request,
    UserManager<ApplicationUser> userManager,
    HttpContext httpContext) =>
{
    var userId = userManager.GetUserId(httpContext.User);
    
    if (string.IsNullOrWhiteSpace(userId))
    {
        return Results.Unauthorized();
    }

    var user = await userManager.FindByIdAsync(userId);
    
    if (user is null)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.NewEmail) || !new EmailAddressAttribute().IsValid(request.NewEmail))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["NewEmail"] = ["A valid email address is required."]
        });
    }

    var changeResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

    if (!changeResult.Succeeded)
    {
        return Results.ValidationProblem(changeResult.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
    }

    var normalizedEmail = request.NewEmail.Trim();
    if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
    {
        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null && !string.Equals(existingUser.Id, user.Id, StringComparison.Ordinal))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["NewEmail"] = ["Email is already in use."]
            });
        }

        var emailToken = await userManager.GenerateChangeEmailTokenAsync(user, normalizedEmail);
        var emailResult = await userManager.ChangeEmailAsync(user, normalizedEmail, emailToken);
        if (!emailResult.Succeeded)
        {
            return Results.ValidationProblem(emailResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        }

        var usernameResult = await userManager.SetUserNameAsync(user, normalizedEmail);
        if (!usernameResult.Succeeded)
        {
            return Results.ValidationProblem(usernameResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        }
    }

    user.MustCompleteOnboarding = false;
    await userManager.UpdateAsync(user);

    return Results.NoContent();
}).RequireAuthorization();

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

public sealed record LoginRequest(string Email, string Password);

public sealed record CreateInvitationRequest(string Email);

public sealed record RegisterWithInvitationRequest(string Email, string Password, string Token);

public sealed record CompleteOnboardingRequest(string CurrentPassword, string NewPassword, string NewEmail);
