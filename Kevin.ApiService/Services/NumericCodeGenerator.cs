namespace Kevin.ApiService.Services;

public class NumericCodeGenerator : ICodeGenerator
{
    public const int Length = 7;

    public string Generate() =>
        Random.Shared.NextInt64((long)Math.Pow(10, Length - 1), (long)Math.Pow(10, Length)).ToString();
}
