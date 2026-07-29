namespace urlShortener.Models
{
    public class Address(Guid id, string originalUrl, Guid userId)
    {
        public Guid Id { get; set; } = id;
        public string OriginalUrl { get; set; } = originalUrl;
        public string NewUrl { get; set; } = string.Empty;
        public Guid UserId { get; set; } = userId;
    }
}
