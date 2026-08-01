using urlShortener.Services;

namespace urlShortener.Tests
{
    public class ClickMetadataServiceTests
    {
        private readonly ClickMetadataService _servico = new();

        // User-agents reais. O ponto sensivel e que os navegadores se identificam
        // uns como os outros: Edge diz "Chrome", Chrome diz "Safari".
        private const string ChromeWindows =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        private const string EdgeWindows =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0";

        private const string FirefoxWindows =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0";

        private const string SafariIphone =
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";

        private const string SafariIpad =
            "Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Safari/604.1";

        private const string ChromeAndroidCelular =
            "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";

        private const string ChromeAndroidTablet =
            "Mozilla/5.0 (Linux; Android 13; SM-X700) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        private const string SafariMac =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Safari/605.1.15";

        [Theory]
        [InlineData(ChromeWindows, "Chrome")]
        [InlineData(EdgeWindows, "Edge")]          // contem "Chrome" e "Safari"
        [InlineData(FirefoxWindows, "Firefox")]
        [InlineData(SafariIphone, "Safari")]
        [InlineData(ChromeAndroidCelular, "Chrome")]
        [InlineData(SafariMac, "Safari")]
        public void Derivar_IdentificaNavegador(string userAgent, string esperado)
        {
            Assert.Equal(esperado, _servico.Derivar(userAgent).Browser);
        }

        [Theory]
        [InlineData(ChromeWindows, "Desktop")]
        [InlineData(SafariIphone, "Mobile")]
        [InlineData(ChromeAndroidCelular, "Mobile")]
        [InlineData(SafariIpad, "Tablet")]
        [InlineData(ChromeAndroidTablet, "Tablet")]  // Android sem "Mobile"
        public void Derivar_IdentificaDispositivo(string userAgent, string esperado)
        {
            Assert.Equal(esperado, _servico.Derivar(userAgent).DeviceType);
        }

        [Theory]
        [InlineData(ChromeWindows, "Windows")]
        [InlineData(SafariIphone, "iOS")]
        [InlineData(SafariIpad, "iOS")]
        [InlineData(ChromeAndroidCelular, "Android")]  // contem "Linux" tambem
        [InlineData(SafariMac, "macOS")]
        public void Derivar_IdentificaSistema(string userAgent, string esperado)
        {
            Assert.Equal(esperado, _servico.Derivar(userAgent).OperatingSystem);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Derivar_SemUserAgent_RetornaDesconhecido(string? userAgent)
        {
            var (dispositivo, navegador, sistema) = _servico.Derivar(userAgent);

            Assert.Equal(ClickMetadataService.Desconhecido, dispositivo);
            Assert.Equal(ClickMetadataService.Desconhecido, navegador);
            Assert.Equal(ClickMetadataService.Desconhecido, sistema);
        }

        // So o host: o caminho da origem pode conter termos de busca e identificadores.
        [Theory]
        [InlineData("https://www.google.com/search?q=algo+privado", "www.google.com")]
        [InlineData("http://exemplo.com.br/pagina/interna", "exemplo.com.br")]
        [InlineData("https://t.co/abc", "t.co")]
        public void ExtrairRefererHost_DevolveApenasOHost(string referer, string esperado)
        {
            Assert.Equal(esperado, _servico.ExtrairRefererHost(referer));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("nao-e-uma-url")]
        [InlineData("/caminho/relativo")]
        public void ExtrairRefererHost_ComValorInvalido_RetornaNull(string? referer)
        {
            Assert.Null(_servico.ExtrairRefererHost(referer));
        }
    }
}
