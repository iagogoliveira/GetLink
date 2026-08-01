using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using urlShortener.Data;
using urlShortener.Models;
using urlShortener.Repositories;
using urlShortener.Services;

namespace urlShortener.Tests
{
    // Uma URL sem esquema gravada crua vira um Location relativo no redirect,
    // e o navegador acaba dentro do proprio encurtador em vez do site de destino.
    public class UrlNormalizationTests
    {
        private static UrlService MontarServico()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UrlShortener:BaseUrl"] = "https://localhost:7000"
                })
                .Build();

            var repositorio = new UrlRepository(new AppDbContext(options));

            return new UrlService(
                repositorio,
                new UrlValidatorService(),
                new UrlGeneratorService(repositorio, config));
        }

        [Theory]
        [InlineData("https://exemplo.com")]
        [InlineData("http://exemplo.com")]
        [InlineData("exemplo.com")]
        public void CheckValidUrl_AceitaHttpHttpsESemEsquema(string url)
        {
            Assert.True(new UrlValidatorService().CheckValidUrl(url));
        }

        [Theory]
        [InlineData("javascript:alert(1)")]
        [InlineData("//evil.com")]
        [InlineData("")]
        [InlineData("   ")]
        public void CheckValidUrl_RejeitaEsquemasPerigososEVazios(string url)
        {
            Assert.False(new UrlValidatorService().CheckValidUrl(url));
        }

        [Fact]
        public async Task CreateNewUrl_GravaUrlSemEsquemaComHttps()
        {
            var servico = MontarServico();

            var criada = await servico.CreateNewUrl(
                new Address(Guid.NewGuid(), "exemplo.com", Guid.NewGuid()));

            Assert.Equal("https://exemplo.com", criada.OriginalUrl);
        }

        [Fact]
        public async Task CreateNewUrl_PreservaUrlQueJaTemEsquema()
        {
            var servico = MontarServico();

            var criada = await servico.CreateNewUrl(
                new Address(Guid.NewGuid(), "http://exemplo.com/caminho", Guid.NewGuid()));

            Assert.Equal("http://exemplo.com/caminho", criada.OriginalUrl);
        }

        [Fact]
        public async Task CreateNewUrl_ComUrlInvalida_LancaInvalidOperation()
        {
            var servico = MontarServico();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => servico.CreateNewUrl(
                    new Address(Guid.NewGuid(), "javascript:alert(1)", Guid.NewGuid())));
        }

        // O valor gravado tem de ser absoluto: e isso que faz o navegador sair
        // do dominio do encurtador ao seguir o Location do 302.
        [Fact]
        public async Task UrlGravada_ResolveParaDominioExterno()
        {
            var servico = MontarServico();

            var criada = await servico.CreateNewUrl(
                new Address(Guid.NewGuid(), "exemplo.com", Guid.NewGuid()));

            var paginaDoRedirect = new Uri("https://localhost:7000/abc123");
            var destino = new Uri(paginaDoRedirect, criada.OriginalUrl);

            Assert.Equal("exemplo.com", destino.Host);
        }
    }
}
