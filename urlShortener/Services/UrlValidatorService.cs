using System.Text.RegularExpressions;

namespace urlShortener.Services
{
    public class UrlValidatorService
    {
        public bool CheckValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(NormalizeUrl(url), UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp
                   || uri.Scheme == Uri.UriSchemeHttps);
        }

        // Precisa ser aplicado ao valor que vai para o banco: sem esquema, o header
        // Location do redirect vira relativo e o navegador nao sai do encurtador.
        public string NormalizeUrl(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                return "https://" + url;
            }

            return url;
        }
    }

}
