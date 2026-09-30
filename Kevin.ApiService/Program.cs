using Asp.Versioning;
using Kevin.ApiService.Data;
using Kevin.ApiService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<UrlDbContext>("urlshortener");

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1.0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new HeaderApiVersionReader("api-version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddMvc();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ICodeGenerator, NumericCodeGenerator>();
builder.Services.AddSingleton<ICodeEncoder, SqidsCodeEncoder>();
builder.Services.AddSingleton<ITargetUrlValidator>(new TargetUrlValidator(builder.Configuration["TRUSTED_DOMAIN"]));
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<UrlDbContext>().Database.EnsureCreatedAsync();
}

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();

