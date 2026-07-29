using System.Text.RegularExpressions;

namespace urlShortener.Services
{
    public class UrlValidatorService
    {
        public bool CheckValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp
                   || uri.Scheme == Uri.UriSchemeHttps);
        }
    }

}
