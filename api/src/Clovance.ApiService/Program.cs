using System.Text.Json.Serialization;
using Clovance.ApiService.Exceptions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Auth.Jwt;
using Clovance.ApiService.Infrastructure.Auth.PasswordReset;
using Clovance.ApiService.Infrastructure.Auth.Refresh;
using Clovance.ApiService.Infrastructure.Auth.UserInvitation;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.Email;
using Clovance.ApiService.Infrastructure.ExternalServices;
using Clovance.ApiService.Infrastructure.Frontend;
using Clovance.ApiService.Infrastructure.HttpRequest;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "es", "en" };
    
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddFrontend(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddMemoryCache();

if (builder.Environment.IsTesting())
{
    builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();
}
else
{
    builder.Services.AddSmtpEmailSender(builder.Configuration);
}

builder.Services.AddRefreshTokenCleanup(builder.Configuration);
builder.Services.AddUserInvitationService(builder.Configuration);
builder.Services.AddPasswordReset(builder.Configuration);
builder.Services.AddHttpClient<ICurrencyConverter, FrankfurterCurrencyConverter>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add HttpContextAccessor for handlers
builder.Services.AddHttpContextAccessor();

// Register handlers
builder.Services.AddHandlersFromAssembly(typeof(Program).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Bearer token"
        };

        return Task.CompletedTask;
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseRequestLocalization();

app.UseHttpRequestTracing();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options => options
        .WithTitle("Clovance API Reference")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .AddPreferredSecuritySchemes("Bearer")
        .AddHttpAuthentication("Bearer", auth =>
        {
            auth.Token = "";
        }));

    app.MapGet("/", () => Results.Redirect("/scalar", permanent: false));
}


using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ClovanceDbContext>();
await dbContext.Database.MigrateAsync();

// Map endpoints
app.RegisterApiEndpointsFromAssembly(typeof(Program).Assembly);

app.MapDefaultEndpoints();

await app.SeedIdentityAsync();

app.Run();

