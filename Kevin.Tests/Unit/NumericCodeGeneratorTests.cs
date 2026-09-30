using Kevin.ApiService.Services;

namespace Kevin.Tests.Unit;

public class NumericCodeGeneratorTests
{
    [Test]
    public void Generate_ReturnsNumericCodeOfFixedLength()
    {
        var code = new NumericCodeGenerator().Generate();

        Assert.That(code, Has.Length.EqualTo(NumericCodeGenerator.Length));
        Assert.That(double.TryParse(code, out _));
    }
}
