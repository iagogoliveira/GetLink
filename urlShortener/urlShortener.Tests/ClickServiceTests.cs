using Microsoft.EntityFrameworkCore;
using urlShortener.Data;
using urlShortener.Models;
using urlShortener.Repositories;
using urlShortener.Services;

namespace urlShortener.Tests
{
    public class ClickServiceTests
    {
        private const string ChromeWindows =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        private static (ClickService servico, AppDbContext contexto) Montar()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var contexto = new AppDbContext(options);
            var servico = new ClickService(new UrlRepository(contexto), new ClickMetadataService());

            return (servico, contexto);
        }

        private static Address NovaUrl(Guid dono, string codigo = "abc123")
        {
            return new Address(Guid.NewGuid(), "https://exemplo.com", dono)
            {
                NewUrl = $"https://localhost:7000/{codigo}"
            };
        }

        [Fact]
        public async Task RegistrarClique_GravaMetadadosDerivados()
        {
            var (servico, contexto) = Montar();
            var url = NovaUrl(Guid.NewGuid());
            contexto.Addresses.Add(url);
            await contexto.SaveChangesAsync();

            await servico.RegistrarClique(url.Id, ChromeWindows, "https://www.google.com/search?q=x");

            var clique = await contexto.Clicks.SingleAsync();

            Assert.Equal("Chrome", clique.Browser);
            Assert.Equal("Desktop", clique.DeviceType);
            Assert.Equal("Windows", clique.OperatingSystem);
            Assert.Equal("www.google.com", clique.RefererHost);
        }

        // O objetivo do design e nao guardar dado pessoal: nenhum campo pode
        // conter o user-agent cru nem o caminho completo do referer.
        [Fact]
        public async Task RegistrarClique_NaoPersisteUserAgentNemUrlDeOrigemCompleta()
        {
            var (servico, contexto) = Montar();
            var url = NovaUrl(Guid.NewGuid());
            contexto.Addresses.Add(url);
            await contexto.SaveChangesAsync();

            await servico.RegistrarClique(url.Id, ChromeWindows, "https://www.google.com/search?q=termo+sensivel");

            var clique = await contexto.Clicks.SingleAsync();
            var campos = new[] { clique.Browser, clique.DeviceType, clique.OperatingSystem, clique.RefererHost ?? "" };

            Assert.DoesNotContain(campos, c => c.Contains("Mozilla"));
            Assert.DoesNotContain(campos, c => c.Contains("AppleWebKit"));
            Assert.DoesNotContain(campos, c => c.Contains("termo+sensivel"));
        }

        [Fact]
        public async Task ListarUrlsDoUsuario_TrazTotalDeCliques()
        {
            var (servico, contexto) = Montar();
            var dono = Guid.NewGuid();
            var url = NovaUrl(dono);
            contexto.Addresses.Add(url);
            await contexto.SaveChangesAsync();

            await servico.RegistrarClique(url.Id, ChromeWindows, null);
            await servico.RegistrarClique(url.Id, ChromeWindows, null);
            await servico.RegistrarClique(url.Id, ChromeWindows, null);

            var lista = await servico.ListarUrlsDoUsuario(dono);

            Assert.Single(lista);
            Assert.Equal(3, lista[0].TotalClicks);
            Assert.NotNull(lista[0].LastClickedAt);
        }

        [Fact]
        public async Task ListarUrlsDoUsuario_UrlSemCliques_TotalZero()
        {
            var (servico, contexto) = Montar();
            var dono = Guid.NewGuid();
            contexto.Addresses.Add(NovaUrl(dono));
            await contexto.SaveChangesAsync();

            var lista = await servico.ListarUrlsDoUsuario(dono);

            Assert.Single(lista);
            Assert.Equal(0, lista[0].TotalClicks);
            Assert.Null(lista[0].LastClickedAt);
        }

        [Fact]
        public async Task ListarUrlsDoUsuario_NaoTrazUrlDeOutroUsuario()
        {
            var (servico, contexto) = Montar();
            var dono = Guid.NewGuid();
            contexto.Addresses.Add(NovaUrl(dono, "minha"));
            contexto.Addresses.Add(NovaUrl(Guid.NewGuid(), "doOutro"));
            await contexto.SaveChangesAsync();

            var lista = await servico.ListarUrlsDoUsuario(dono);

            Assert.Single(lista);
            Assert.EndsWith("minha", lista[0].ShortUrl);
        }

        [Fact]
        public async Task ObterEstatisticas_DeOutroUsuario_RetornaNull()
        {
            var (servico, contexto) = Montar();
            var url = NovaUrl(Guid.NewGuid());
            contexto.Addresses.Add(url);
            await contexto.SaveChangesAsync();

            Assert.Null(await servico.ObterEstatisticas(url.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task ObterEstatisticas_ComIdInexistente_RetornaNull()
        {
            var (servico, _) = Montar();

            Assert.Null(await servico.ObterEstatisticas(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ObterEstatisticas_AgrupaPorNavegadorEOrigem()
        {
            var (servico, contexto) = Montar();
            var dono = Guid.NewGuid();
            var url = NovaUrl(dono);
            contexto.Addresses.Add(url);
            await contexto.SaveChangesAsync();

            const string firefox =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0";

            await servico.RegistrarClique(url.Id, ChromeWindows, "https://google.com/x");
            await servico.RegistrarClique(url.Id, ChromeWindows, "https://google.com/y");
            await servico.RegistrarClique(url.Id, firefox, null);

            var stats = await servico.ObterEstatisticas(url.Id, dono);

            Assert.NotNull(stats);
            Assert.Equal(3, stats.TotalClicks);

            // Ordenado por total decrescente.
            Assert.Equal("Chrome", stats.PorNavegador[0].Chave);
            Assert.Equal(2, stats.PorNavegador[0].Total);

            Assert.Equal("google.com", stats.PorOrigem[0].Chave);
            Assert.Equal(2, stats.PorOrigem[0].Total);

            // Sem referer e categoria propria, nao dado faltante.
            Assert.Contains(stats.PorOrigem, o => o.Chave == "Acesso direto" && o.Total == 1);
        }

        [Fact]
        public async Task ObterEstatisticas_AgrupaPorDia()
        {
            var (servico, contexto) = Montar();
            var dono = Guid.NewGuid();
            var url = NovaUrl(dono);
            contexto.Addresses.Add(url);
            await contexto.SaveChangesAsync();

            await servico.RegistrarClique(url.Id, ChromeWindows, null);
            await servico.RegistrarClique(url.Id, ChromeWindows, null);

            var stats = await servico.ObterEstatisticas(url.Id, dono);

            Assert.NotNull(stats);
            Assert.Single(stats.ClicksPorDia);
            Assert.Equal(2, stats.ClicksPorDia[0].Total);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), stats.ClicksPorDia[0].Dia);
        }
    }
}
