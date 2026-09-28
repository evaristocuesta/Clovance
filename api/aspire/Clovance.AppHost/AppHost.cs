using Aspire.Hosting.Docker.Resources.ServiceNodes;

var builder = DistributedApplication.CreateBuilder(args);

var isTestEnvironment = builder.Environment.EnvironmentName == "Testing";

builder.AddDockerComposeEnvironment("env");

var postgresUsername = builder.AddParameter("postgres-username");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

// --- SMTP ---
var smtpHost = builder.AddParameter("smtp-host");
var smtpPort = builder.AddParameter("smtp-port");
var smtpUsername = builder.AddParameter("smtp-username");
var smtpPassword = builder.AddParameter("smtp-password", secret: true);
var smtpFromAddress = builder.AddParameter("smtp-from-address");
var smtpFromName = builder.AddParameter("smtp-from-name");

var frontendBaseUrl = builder.AddParameter("frontend-base-url");

var volumeSourcePath = builder.AddParameter("volume-source-path");

// Use different resource names for testing vs development to avoid container conflicts
var postgresResourceName =
    isTestEnvironment ? "clovance-postgres-test" : "clovance-postgres";

var postgres = builder
    .AddPostgres(
        postgresResourceName,
        userName: postgresUsername,
        password: postgresPassword)
    // Set the name of the default database to auto-create on container startup.
    .WithEnvironment("POSTGRES_DB", "clovance-database")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Restart = "unless-stopped";

        if (!isTestEnvironment)
        {
            service.Volumes.Add(new Volume
            {
                Name = "clovance-postgres-data",
                Source = $"{volumeSourcePath.AsEnvironmentPlaceholder(resource)}/postgres",
                Target = "/var/lib/postgresql",
                Type = "bind",
                ReadOnly = false
            });
        }
    });

if (!isTestEnvironment)
{
    // In development: persist data and keep container running
    postgres
        .WithLifetime(ContainerLifetime.Persistent)
        .WithPgWeb();
}
// In testing: ephemeral container with no volume (destroyed after tests)

// Add the default database to the application model so that it can be referenced by other resources.
var database = postgres.AddDatabase("clovance-database");

var jwtKeyFilePath =
    builder.Configuration["Jwt:KeyFilePath"] ?? "/home/app/jwt.key";

var apiService = builder
    .AddProject<Projects.Clovance_ApiService>("clovance-apiservice")
    .WithReference(database)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithEnvironment("Jwt__KeyFilePath", jwtKeyFilePath)
    .WithEnvironment("Smtp__Host", smtpHost)
    .WithEnvironment("Smtp__Port", smtpPort)
    .WithEnvironment("Smtp__Username", smtpUsername)
    .WithEnvironment("Smtp__Password", smtpPassword)
    .WithEnvironment("Smtp__FromAddress", smtpFromAddress)
    .WithEnvironment("Smtp__FromName", smtpFromName)
    .WithEnvironment("Frontend__BaseUrl", frontendBaseUrl)
    .WaitFor(database)
    .WithHttpHealthCheck("/health")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Volumes.Add(new Volume
        {
            Name = "clovance-jwt-keys",
            Source = $"{volumeSourcePath.AsEnvironmentPlaceholder(resource)}/jwt",
            Target = "/home/app",
            Type = "bind",
            ReadOnly = false
        });

        service.Restart = "unless-stopped";
    });

builder
    .AddJavaScriptApp(
        "clovance-frontend",
        "../../../frontend",
        runScriptName: "start")
    .WithPnpm(installArgs: ["--frozen-lockfile", "--ignore-scripts"])
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithHttpEndpoint(port: 7000, env: "PORT")
    .WithHttpHealthCheck("/healthz.txt")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile(container => container
        .WithEntrypoint("/docker-entrypoint.sh")
        .WithArgs("nginx", "-g", "daemon off;"))
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Restart = "unless-stopped";
    });

await builder.Build().RunAsync();
