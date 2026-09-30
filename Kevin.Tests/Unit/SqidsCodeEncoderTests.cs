using Kevin.ApiService.Services;

namespace Kevin.Tests.Unit;

public class SqidsCodeEncoderTests
{
    private readonly SqidsCodeEncoder _encoder = new();

    [Test]
    public void Encode_ReturnsBase62CodeOfMinimumLength()
    {
        var code = _encoder.Encode(1);

        Assert.That(code, Has.Length.GreaterThanOrEqualTo(SqidsCodeEncoder.MinLength));
        Assert.That(code, Does.Match("^[0-9a-zA-Z]+$"));
    }

    [Test]
    public void Encode_ReturnsUniqueCodes_ForSequentialIds()
    {
        var codes = Enumerable.Range(1, 100_000).Select(id => _encoder.Encode(id)).ToList();

        Assert.That(codes, Is.Unique);
    }


}
