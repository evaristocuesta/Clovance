using Aspire.Hosting.Docker.Resources.ServiceNodes;

var builder = DistributedApplication.CreateBuilder(args);

var isTestEnvironment = builder.Environment.EnvironmentName == "Testing";

builder.AddDockerComposeEnvironment("env")
        .ConfigureComposeFile(composeFile =>
        {
            composeFile.AddVolume(new Volume
            {
                Name = "clovance-jwt-keys",
                Driver = "local"
            });
        }); ;

var postgresUsername = builder.AddParameter("postgres-username");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

// Use different resource names for testing vs development to avoid container conflicts
var postgresResourceName = isTestEnvironment ? "clovance-postgres-test" : "clovance-postgres";

var postgres = builder.AddPostgres(postgresResourceName, userName: postgresUsername, password: postgresPassword)
    // Set the name of the default database to auto-create on container startup.
    .WithEnvironment("POSTGRES_DB", "clovance-database");
// Mount the SQL scripts directory into the container so that the init scripts run.
//.WithBindMount("../DatabaseContainers.ApiService/data/postgres", "/docker-entrypoint-initdb.d")

if (!isTestEnvironment)
{
    // In development: persist data and keep container running
    postgres
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent)
        .WithPgWeb();
}
// In testing: ephemeral container with no volume (destroyed after tests)

// Add the default database to the application model so that it can be referenced by other resources.
var database = postgres.AddDatabase("clovance-database");

var jwtKeyFilePath = builder.Configuration["Jwt:KeyFilePath"] ?? "/home/app/jwt.key";

var apiService = builder.AddProject<Projects.Clovance_ApiService>("clovance-apiservice")
    .WithReference(database)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithEnvironment("Jwt__KeyFilePath", jwtKeyFilePath)
    .WaitFor(database)
    .WithHttpHealthCheck("/health")
    .PublishAsDockerComposeService((resource, service) =>
    {
        var volume = new Aspire.Hosting.Docker.Resources.ServiceNodes.Volume
        {
            Name = "clovance-jwt-keys",
            Source = "clovance-jwt-keys",
            Target = "/home/app",
            Type = "volume", 
            ReadOnly = false,
        };

        service.Volumes.Add(volume);
    });

builder.AddJavaScriptApp("clovance-frontend", "../../../frontend", runScriptName: "start")
    .WithPnpm(installArgs: ["--frozen-lockfile", "--ignore-scripts"])
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithHttpEndpoint(port: 7000, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile(container => container
        .WithEntrypoint("/docker-entrypoint.sh")
        .WithArgs("nginx", "-g", "daemon off;"));

await builder.Build().RunAsync();
