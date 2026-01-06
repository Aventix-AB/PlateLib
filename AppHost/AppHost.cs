var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

var API = builder.AddProject<Projects.API>("api")
    .WithReference(postgresdb);

builder.Build().Run();