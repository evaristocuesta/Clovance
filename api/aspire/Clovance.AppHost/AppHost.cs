var builder = DistributedApplication.CreateBuilder(args);

// Add the following line to configure the Docker Compose environment
builder.AddDockerComposeEnvironment("env");

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    // Set the name of the default database to auto-create on container startup.
    .WithEnvironment("POSTGRES_DB", "database")
    // Mount the SQL scripts directory into the container so that the init scripts run.
    //.WithBindMount("../DatabaseContainers.ApiService/data/postgres", "/docker-entrypoint-initdb.d")
    // Configure the container to store data in a volume so that it persists across instances.
    .WithDataVolume()
    .WithPgWeb()
    // Keep the container running between app host sessions.
    .WithLifetime(ContainerLifetime.Persistent);

// Add the default database to the application model so that it can be referenced by other resources.
var database = postgres.AddDatabase("database");

var apiService = builder.AddProject<Projects.Clovance_ApiService>("apiservice")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(database)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.AddJavaScriptApp("frontend", "../../../frontend", runScriptName: "start")
    .WithPnpm(installArgs: ["--frozen-lockfile", "--ignore-scripts"])
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

await builder.Build().RunAsync();
