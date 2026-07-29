using urlShortener.Repositories;

namespace urlShortener.Services
{
    public class UrlGeneratorService
    {
        private readonly IUrlRepository _urlRepository;
        private readonly IConfiguration _configuration;


        public UrlGeneratorService(IUrlRepository urlRepository, IConfiguration configuration)
        {
            _urlRepository = urlRepository;
            _configuration = configuration;
        }

        public async Task<string> GenerateFullUrl()
        {
            string urlPath;
            string fullUrl;
            do
            {
                urlPath = GenerateUrlPath();
                fullUrl = FormatUrl(urlPath);
            }
            while (await _urlRepository.ExistsAsync(fullUrl));
            return fullUrl;
        }

        public async Task<string> GenerateCustomPath(string customPath)
        {
            string newUrl = FormatUrl(customPath);
            if (!await _urlRepository.ExistsAsync(newUrl))
            {
                return newUrl;
            }
            throw new InvalidOperationException("Custom path already exists.");
        }

        private string GenerateUrlPath()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string FormatUrl(string path)
        {
            string? baseUrl = _configuration["UrlShortener:BaseUrl"];
            string newUrl = string.Empty;
            return newUrl = $"{baseUrl}/{path}";

        }
    }

}
