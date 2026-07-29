namespace urlShortener.DTOs
{
    public class CreateNewUrlDto
    {
        public string OriginalUrl { get; set; }
        public Guid UserId { get; set; }
    }
}
