var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgres.AddDatabase("urlshortener");

builder.AddProject<Projects.Kevin_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health", endpointName: "http")
    .WithUrlForEndpoint("http", _ => new() { Url = "/scalar/v1", DisplayText = "Scalar" })
    .WithExternalHttpEndpoints();

builder.Build().Run();
