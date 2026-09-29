using Kevin.ApiService.Contracts.V1;
using Kevin.ApiService.Controllers.V1;
using Kevin.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kevin.Tests.Unit;

public class ShortUrlsControllerTests
{
    private Mock<IUrlShortenerService> _service = null!;
    private ShortUrlsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IUrlShortenerService>();
        _controller = new ShortUrlsController(_service.Object);
    }

    [Test]
    public async Task Shorten_ReturnsCreatedWithCode()
    {
        _service.Setup(x => x.ShortenAsync("https://example.com", It.IsAny<CancellationToken>())).ReturnsAsync("12345678");

        var result = await _controller.Shorten(new ShortenRequest("https://example.com"), CancellationToken.None);

        var created = result.Result as CreatedResult;
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Location, Is.EqualTo("/12345678"));
        Assert.That(created.Value, Is.EqualTo(new ShortenResponse("12345678")));
    }

    [Test]
    public async Task Visit_RedirectsToLongUrl()
    {
        _service.Setup(x => x.ResolveAsync("12345678", It.IsAny<CancellationToken>())).ReturnsAsync("https://example.com");

        var result = await _controller.Visit("12345678", CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectResult>());
        Assert.That(((RedirectResult)result).Url, Is.EqualTo("https://example.com"));
    }

    [Test]
    public async Task Visit_ReturnsNotFound_WhenCodeIsUnknown()
    {
        var result = await _controller.Visit("00000000", CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Stats_ReturnsVisitCount()
    {
        _service.Setup(x => x.GetVisitsAsync("12345678", It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var result = await _controller.Stats("12345678", CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(new StatsResponse("12345678", 5)));
    }

    [Test]
    public async Task Stats_ReturnsNotFound_WhenCodeIsUnknown()
    {
        var result = await _controller.Stats("00000000", CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        _service.Setup(x => x.DeleteAsync("12345678", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _controller.Delete("12345678", CancellationToken.None);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_ReturnsNotFound_WhenCodeIsUnknown()
    {
        var result = await _controller.Delete("00000000", CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }
}
