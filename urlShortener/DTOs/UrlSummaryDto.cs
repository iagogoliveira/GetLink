namespace urlShortener.DTOs
{
    public class UrlSummaryDto
    {
        public Guid Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalClicks { get; set; }
        public DateTime? LastClickedAt { get; set; }
    }
}
