namespace UrlShortener.Models
{
    public class ShortUrl
    {
        public int Id { get; set; } // Primary key
        public string Name { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ClickCount { get; set; } = 0;
    }
}
