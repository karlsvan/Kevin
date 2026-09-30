namespace Kevin.ApiService.Services;

// Allows rooted relative paths, and absolute http(s) urls on the trusted domain or its subdomains.
// Without a trusted domain, only relative paths are allowed.
public class TargetUrlValidator(string? trustedDomain) : ITargetUrlValidator
{
    private readonly string? _trustedDomain = string.IsNullOrWhiteSpace(trustedDomain) ? null : trustedDomain.Trim();

    public bool IsAllowed(string url)
    {
        // browsers strip tabs/newlines from urls, which could turn a harmless looking url into a different one
        if (string.IsNullOrEmpty(url) || url.Any(c => char.IsControl(c) || char.IsWhiteSpace(c)))
        {
            return false;
        }

        return url.StartsWith('/') ? IsRelative(url) : IsTrustedAbsolute(url);
    }

    // browsers treat "//host" and "/\host" as protocol-relative urls to another host
    private static bool IsRelative(string url) =>
        url.Length == 1 || (url[1] != '/' && url[1] != '\\');

    private bool IsTrustedAbsolute(string url)
    {
        if (_trustedDomain is null
            || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        return uri.Host.Equals(_trustedDomain, StringComparison.OrdinalIgnoreCase)
               || uri.Host.EndsWith("." + _trustedDomain, StringComparison.OrdinalIgnoreCase);
    }
}
