var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithImage("postgres:18").WithHostPort(5439);
var postgresdb = postgres.AddDatabase("postgresdb");

var migrations = builder.AddProject<Projects.MigrationService>("migrations")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var API = builder.AddProject<Projects.API>("api")
    .WithReference(postgresdb)
    .WithReference(migrations)
    .WaitForCompletion(migrations);

builder.Build().Run();