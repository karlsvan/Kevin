namespace Kevin.ApiService.Services;

public interface ITargetUrlValidator
{
    bool IsAllowed(string url);
}
