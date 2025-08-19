namespace UrlShortener.Services
{
    public interface IUrlValidator
    {
        bool IsValidUrl(string url);
        bool TryNormalize(string url, out string normalizedUrl);
    }
}
