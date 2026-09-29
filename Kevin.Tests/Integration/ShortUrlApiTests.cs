using System.Net.Http.Json;
using Aspire.Hosting;
using Kevin.ApiService.Contracts.V1;
using Microsoft.Extensions.Logging;

namespace Kevin.Tests.Integration;

[Category("Integration")]
public class ShortUrlApiTests
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

    private DistributedApplication _app = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        using var cts = new CancellationTokenSource(StartupTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Kevin_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appHost.BuildAsync(cancellationToken);
        await _app.StartAsync(cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken);

        _client = _app.CreateHttpClient("apiservice", "http");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Test]
    public async Task Shorten_ReturnsNumericCode()
    {
        using var response = await _client.PostAsJsonAsync("/shorten", new ShortenRequest("https://example.com"));
        var body = await response.Content.ReadFromJsonAsync<ShortenResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(body!.Code, Does.Match("^[0-9]+$"));
        Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo($"/{body.Code}"));
    }

    [Test]
    public async Task Shorten_ReturnsBadRequest_ForInvalidUrl()
    {
        using var response = await _client.PostAsJsonAsync("/shorten", new ShortenRequest("not a url"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Visit_RedirectsToLongUrl()
    {
        var code = await ShortenAsync("https://example.com/redirect");

        using var response = await _client.GetAsync($"/{code}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location, Is.EqualTo(new Uri("https://example.com/redirect")));
    }

    [Test]
    public async Task Stats_ReturnsNumberOfVisits()
    {
        var code = await ShortenAsync("https://example.com/stats");
        (await _client.GetAsync($"/{code}")).Dispose();
        (await _client.GetAsync($"/{code}")).Dispose();

        var stats = await _client.GetFromJsonAsync<StatsResponse>($"/{code}/stats");

        Assert.That(stats, Is.EqualTo(new StatsResponse(code, 2)));
    }

    [Test]
    public async Task Delete_RemovesCode()
    {
        var code = await ShortenAsync("https://example.com/delete");

        using var deleteResponse = await _client.DeleteAsync($"/{code}");
        using var visitResponse = await _client.GetAsync($"/{code}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(visitResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UnknownCode_ReturnsNotFound()
    {
        using var visitResponse = await _client.GetAsync("/00000000");
        using var statsResponse = await _client.GetAsync("/00000000/stats");
        using var deleteResponse = await _client.DeleteAsync("/00000000");

        Assert.That(visitResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(statsResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase("/shorten")]
    [TestCase("/shorten?api-version=1.0")]
    public async Task Shorten_UsesVersion1_ByDefaultAndWhenRequested(string path)
    {
        using var response = await _client.PostAsJsonAsync(path, new ShortenRequest("https://example.com"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.GetValues("api-supported-versions"), Does.Contain("1.0"));
    }

    [Test]
    public async Task Shorten_ReturnsBadRequest_ForUnsupportedVersion()
    {
        using var response = await _client.PostAsJsonAsync("/shorten?api-version=2.0", new ShortenRequest("https://example.com"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ApiReference_IsServed()
    {
        var openApi = await _client.GetStringAsync("/openapi/v1.json");
        using var scalar = await _client.GetAsync("/scalar/v1");

        Assert.That(openApi, Does.Contain("/shorten").And.Contain("/{code}/stats"));
        Assert.That(scalar.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private async Task<string> ShortenAsync(string url)
    {
        using var response = await _client.PostAsJsonAsync("/shorten", new ShortenRequest(url));
        var body = await response.Content.ReadFromJsonAsync<ShortenResponse>();
        return body!.Code;
    }
}
