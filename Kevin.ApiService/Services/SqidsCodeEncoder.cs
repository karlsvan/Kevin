using Sqids;

namespace Kevin.ApiService.Services;


public class SqidsCodeEncoder : ICodeEncoder
{
    public const int MinLength = 5;
    private readonly SqidsEncoder<long> _encoder;

    public SqidsCodeEncoder()
    {
        var options = new SqidsOptions { MinLength = MinLength };
        // block codes that are also routes
        options.BlockList.Add("shorten");
        _encoder = new SqidsEncoder<long>(options);
    }

    public string Encode(long id) => _encoder.Encode(id);
}
