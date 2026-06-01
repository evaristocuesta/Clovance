using Clovance.ApiService.Exceptions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.HttpRequest;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
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

var app = builder.Build();

app.UseHttpRequestTracing();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseOnboardingEnforcement();
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

// Apply migrations in Development and Testing environments
if (app.Environment.IsDevelopment() || app.Environment.IsTesting())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClovanceDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Map endpoints
app.RegisterApiEndpointsFromAssembly(typeof(Program).Assembly);

app.MapDefaultEndpoints();

await app.SeedIdentityAsync();

app.Run();

