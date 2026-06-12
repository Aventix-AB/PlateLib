var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithImage("postgres:18").WithHostPort(5439);
var postgresdb = postgres.AddDatabase("postgresdb");

var minio = builder.AddContainer("minio", "minio/minio")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "s3")
    .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "console");

var minioEndpoint = minio.GetEndpoint("s3");

var migrations = builder.AddProject<Projects.MigrationService>("migrations")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WaitFor(minio)
    .WithEnvironment("Storage__ServiceUrl", minioEndpoint)
    .WithEnvironment("Storage__AccessKey", "minioadmin")
    .WithEnvironment("Storage__SecretKey", "minioadmin")
    .WithEnvironment("Storage__ForcePathStyle", "true");

var API = builder.AddProject<Projects.API>("api")
    .WithReference(postgresdb)
    .WithReference(migrations)
    .WaitForCompletion(migrations)
    .WithEnvironment("Storage__ServiceUrl", minioEndpoint)
    .WithEnvironment("Storage__AccessKey", "minioadmin")
    .WithEnvironment("Storage__SecretKey", "minioadmin")
    .WithEnvironment("Storage__ForcePathStyle", "true");

var Web = builder.AddViteApp("web", "../Web")
    .WithPnpm();


builder.Build().Run();