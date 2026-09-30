using Asp.Versioning;
using Kevin.ApiService.Contracts.V1;
using Kevin.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kevin.ApiService.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
public class ShortUrlsController(IUrlShortenerService service) : ControllerBase
{
    [HttpPost("shorten")]
    public async Task<ActionResult<ShortenResponse>> Shorten(ShortenRequest request, CancellationToken cancellationToken)
    {
        var code = await service.ShortenAsync(request.Url, request.Format, cancellationToken);
        return Created($"/{code}", new ShortenResponse(code));
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> Visit(string code, CancellationToken cancellationToken)
    {
        var longUrl = await service.ResolveAsync(code, cancellationToken);
        return longUrl is null ? NotFound() : Redirect(longUrl);
    }

    [HttpGet("{code}/stats")]
    public async Task<ActionResult<StatsResponse>> Stats(string code, CancellationToken cancellationToken)
    {
        var visits = await service.GetVisitsAsync(code, cancellationToken);
        return visits is null ? NotFound() : new StatsResponse(code, visits.Value);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code, CancellationToken cancellationToken)
    {
        return await service.DeleteAsync(code, cancellationToken) ? NoContent() : NotFound();
    }
}
