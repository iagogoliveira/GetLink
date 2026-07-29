namespace urlShortener.DTOs
{
    public class UpdateUrlDto
    {
        public Guid Id { get; set; }
        public string OriginalUrl { get; set; }
        public string NewPath { get; set; } = string.Empty;
    }
}
