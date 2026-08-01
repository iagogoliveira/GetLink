namespace urlShortener.Services
{
    /// <summary>
    /// Converte user-agent e referer em metadados nao-pessoais. O user-agent cru
    /// entra aqui e nao sai: so as categorias derivadas sao persistidas.
    ///
    /// A deteccao e heuristica por natureza. Navegadores se identificam com
    /// strings herdadas umas das outras por compatibilidade historica (todo
    /// browser moderno diz "Mozilla", o Chrome diz "Safari", o Edge diz "Chrome"),
    /// entao a ordem das verificacoes abaixo importa: do mais especifico para o
    /// mais generico. Serve para tendencia, nao para contagem exata.
    /// </summary>
    public class ClickMetadataService
    {
        public const string Desconhecido = "Desconhecido";

        public (string DeviceType, string Browser, string OperatingSystem) Derivar(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return (Desconhecido, Desconhecido, Desconhecido);
            }

            return (DetectarDispositivo(userAgent), DetectarNavegador(userAgent), DetectarSistema(userAgent));
        }

        /// <summary>
        /// Extrai so o host do referer. Retorna null quando ausente ou malformado.
        /// </summary>
        public string? ExtrairRefererHost(string? referer)
        {
            if (string.IsNullOrWhiteSpace(referer))
            {
                return null;
            }

            if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            {
                return null;
            }

            return string.IsNullOrEmpty(uri.Host) ? null : uri.Host;
        }

        private static string DetectarDispositivo(string ua)
        {
            // Tablet antes de mobile: o Android de tablet tambem diz "Android",
            // e so se distingue pela ausencia de "Mobile".
            if (Contem(ua, "iPad") || Contem(ua, "Tablet") ||
                (Contem(ua, "Android") && !Contem(ua, "Mobile")))
            {
                return "Tablet";
            }

            if (Contem(ua, "Mobi") || Contem(ua, "iPhone") || Contem(ua, "iPod") ||
                Contem(ua, "Android") || Contem(ua, "Windows Phone"))
            {
                return "Mobile";
            }

            return "Desktop";
        }

        private static string DetectarNavegador(string ua)
        {
            // Edge diz "Chrome" e "Safari"; Opera diz "Chrome"; Chrome diz
            // "Safari". Por isso os derivados vem antes dos originais.
            if (Contem(ua, "Edg")) return "Edge";
            if (Contem(ua, "OPR") || Contem(ua, "Opera")) return "Opera";
            if (Contem(ua, "SamsungBrowser")) return "Samsung Internet";
            if (Contem(ua, "Firefox")) return "Firefox";
            if (Contem(ua, "Chrome") || Contem(ua, "CriOS")) return "Chrome";
            if (Contem(ua, "Safari")) return "Safari";

            return Desconhecido;
        }

        private static string DetectarSistema(string ua)
        {
            // "Android" contem "Linux", entao vem antes.
            if (Contem(ua, "Android")) return "Android";
            if (Contem(ua, "iPhone") || Contem(ua, "iPad") || Contem(ua, "iPod")) return "iOS";
            if (Contem(ua, "Windows")) return "Windows";
            if (Contem(ua, "Mac OS X") || Contem(ua, "Macintosh")) return "macOS";
            if (Contem(ua, "Linux")) return "Linux";

            return Desconhecido;
        }

        private static bool Contem(string origem, string trecho) =>
            origem.Contains(trecho, StringComparison.OrdinalIgnoreCase);
    }
}
