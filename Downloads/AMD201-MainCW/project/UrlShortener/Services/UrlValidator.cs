using System;

namespace UrlShortener.Services
{
    public class UrlValidator : IUrlValidator
    {
        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public bool TryNormalize(string url, out string normalizedUrl)
        {
            normalizedUrl = string.Empty;

            if (!IsValidUrl(url))
                return false;

            // Normalize to ensure consistent format
            var uri = new Uri(url);
            normalizedUrl = uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
            return true;
        }
    }
}
