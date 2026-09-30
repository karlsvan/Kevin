using System.Net.Http.Json;
using Aspire.Hosting;
using Kevin.ApiService.Contracts.V1;
using Kevin.ApiService.Services;
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
    public async Task Shorten_ReturnsNumericCode_WhenFormatIsOmitted()
    {
        using var response = await _client.PostAsJsonAsync("/shorten", new { url = "https://example.com" });
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

    [Test]
    public async Task Shorten_ReturnsAlphanumCode_WhenRequested()
    {
        using var response = await _client.PostAsJsonAsync("/shorten", new { url = "https://example.com", format = "alphanum" });
        var body = await response.Content.ReadFromJsonAsync<ShortenResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(body!.Code, Does.Match("^[0-9a-zA-Z]{5,}$"));
        Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo($"/{body.Code}"));
    }

    [Test]
    public async Task Shorten_Alphanum_ReturnsDifferentCodes_ForSameUrl()
    {
        var first = await ShortenAsync("https://example.com/same", CodeFormat.Alphanum);
        var second = await ShortenAsync("https://example.com/same", CodeFormat.Alphanum);

        Assert.That(second, Is.Not.EqualTo(first));
    }

    [Test]
    public async Task AlphanumCode_CanBeVisitedCountedAndDeleted()
    {
        var code = await ShortenAsync("https://example.com/alphanum", CodeFormat.Alphanum);

        using var visitResponse = await _client.GetAsync($"/{code}");
        var stats = await _client.GetFromJsonAsync<StatsResponse>($"/{code}/stats");
        using var deleteResponse = await _client.DeleteAsync($"/{code}");

        Assert.That(visitResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(visitResponse.Headers.Location, Is.EqualTo(new Uri("https://example.com/alphanum")));
        Assert.That(stats, Is.EqualTo(new StatsResponse(code, 1)));
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [TestCase("\"hex\"")]
    [TestCase("5")]
    public async Task Shorten_ReturnsBadRequest_ForUnknownFormat(string format)
    {
        using var content = new StringContent($$"""{"url": "https://example.com", "format": {{format}}}""", System.Text.Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/shorten", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }


    private async Task<string> ShortenAsync(string url, CodeFormat format = CodeFormat.Numeric)
    {
        using var response = await _client.PostAsJsonAsync("/shorten", new ShortenRequest(url, format));
        var body = await response.Content.ReadFromJsonAsync<ShortenResponse>();
        return body!.Code;
    }
}
