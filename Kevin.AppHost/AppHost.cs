var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard => dashboard
        .WithHostPort(18888)
        .WithEnvironment("ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true"));

var environment = builder.AddParameter("environment", "Development");
var trustedDomain = builder.AddParameter("trusted-domain", "example.com");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgres.AddDatabase("urlshortener");

builder.AddProject<Projects.Kevin_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("TRUSTED_DOMAIN", trustedDomain)
    .WithHttpHealthCheck("/health", endpointName: "http")
    .WithUrlForEndpoint("http", _ => new() { Url = "/scalar/v1", DisplayText = "Scalar" })
    .WithExternalHttpEndpoints()
    //this could be skipped if we could assume all developers had aspire locally
    .PublishAsDockerFile(container => container.WithDockerfile("..", "Kevin.ApiService/Dockerfile"))
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Image = "kevin-apiservice";
        service.Build = new() { Context = ".", Dockerfile = "Kevin.ApiService/Dockerfile" };
        service.Ports = ["8080:8080"];
        service.Environment["ASPNETCORE_ENVIRONMENT"] = environment.AsEnvironmentPlaceholder(resource);
    });

builder.Build().Run();
