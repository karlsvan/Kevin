using Kevin.ApiService.Data;
using Kevin.ApiService.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kevin.Tests.Unit;

public class UrlShortenerServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private Mock<IShortUrlRepository> _repository = null!;
    private Mock<ICodeGenerator> _codeGenerator = null!;
    private Mock<ICodeEncoder> _codeEncoder = null!;
    private Mock<TimeProvider> _timeProvider = null!;
    private UrlShortenerService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IShortUrlRepository>();
        _codeGenerator = new Mock<ICodeGenerator>();
        _codeEncoder = new Mock<ICodeEncoder>();
        _timeProvider = new Mock<TimeProvider>();
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(Now);

        _service = new UrlShortenerService(
            _repository.Object,
            _codeGenerator.Object,
            _codeEncoder.Object,
            _timeProvider.Object,
            NullLogger<UrlShortenerService>.Instance);
    }

    [Test]
    public async Task ShortenAsync_Numeric_StoresUrlUnderGeneratedCode()
    {
        _repository.Setup(x => x.NextIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(42);
        _codeGenerator.Setup(x => x.Generate()).Returns("12345678");

        var code = await _service.ShortenAsync("https://example.com");

        Assert.That(code, Is.EqualTo("12345678"));
        _repository.Verify(x => x.AddAsync(
            It.Is<ShortUrl>(s => s.Id == 42 && s.Code == "12345678" && s.LongUrl == "https://example.com" && s.CreatedAt == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShortenAsync_Numeric_GeneratesNewCode_WhenCodeAlreadyExists()
    {
        _codeGenerator.SetupSequence(x => x.Generate()).Returns("11111111").Returns("22222222");
        _repository.Setup(x => x.ExistsAsync("11111111", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var code = await _service.ShortenAsync("https://example.com");

        Assert.That(code, Is.EqualTo("22222222"));
        _codeGenerator.Verify(x => x.Generate(), Times.Exactly(2));
    }


    [Test]
    public async Task ResolveAsync_ReturnsLongUrl_AndCountsVisit()
    {
        _repository.Setup(x => x.GetAsync("12345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShortUrl { Code = "12345678", LongUrl = "https://example.com" });

        var longUrl = await _service.ResolveAsync("12345678");

        Assert.That(longUrl, Is.EqualTo("https://example.com"));
        _repository.Verify(x => x.IncrementVisitsAsync("12345678", It.IsAny<CancellationToken>()), Times.Once);
    }


    [Test]
    public async Task GetVisitsAsync_ReturnsVisitCount()
    {
        _repository.Setup(x => x.GetAsync("12345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShortUrl { Code = "12345678", LongUrl = "https://example.com", Visits = 3 });

        var visits = await _service.GetVisitsAsync("12345678");

        Assert.That(visits, Is.EqualTo(3));
    }

    [Test]
    public async Task GetVisitsAsync_ReturnsNull_WhenCodeIsUnknown()
    {
        var visits = await _service.GetVisitsAsync("00000000");

        Assert.That(visits, Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task DeleteAsync_ReturnsWhetherCodeWasDeleted(bool deleted)
    {
        _repository.Setup(x => x.DeleteAsync("12345678", It.IsAny<CancellationToken>())).ReturnsAsync(deleted);

        var result = await _service.DeleteAsync("12345678");

        Assert.That(result, Is.EqualTo(deleted));
    }
}
