namespace UrlShortener.Models
{
    public class ShortenUrlRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string? CustomAlias { get; set; } // Optional custom short code
    }
}
