using Kevin.ApiService.Services;

namespace Kevin.Tests.Unit;

public class TargetUrlValidatorTests
{
    private readonly TargetUrlValidator _validator = new("example.com");

    [TestCase("https://example.com/path", ExpectedResult = true)]
    [TestCase("https://www.example.com", ExpectedResult = true)]
    [TestCase("/some/page", ExpectedResult = true)]
    [TestCase("https://evil.com", ExpectedResult = false)]
    [TestCase("https://example.com@evil.com", ExpectedResult = false)]
    [TestCase("//evil.com", ExpectedResult = false)]
    public bool IsAllowed(string url) => _validator.IsAllowed(url);
}
