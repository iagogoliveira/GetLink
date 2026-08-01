using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using urlShortener.Data;
using urlShortener.Models;
using urlShortener.Repositories;
using urlShortener.Services;

namespace urlShortener.Tests
{
    // Um usuario autenticado nao pode alterar nem apagar a URL de outro.
    public class UrlServiceOwnershipTests
    {
        private static readonly Guid Dono = Guid.NewGuid();
        private static readonly Guid Invasor = Guid.NewGuid();

        private static (UrlService servico, AppDbContext contexto) Montar()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var contexto = new AppDbContext(options);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UrlShortener:BaseUrl"] = "https://localhost:7000"
                })
                .Build();

            var repositorio = new UrlRepository(contexto);
            var servico = new UrlService(
                repositorio,
                new UrlValidatorService(),
                new UrlGeneratorService(repositorio, config));

            return (servico, contexto);
        }

        private static async Task<(UrlService servico, AppDbContext contexto, Guid id)> ComUrlDoDono()
        {
            var (servico, contexto) = Montar();

            var url = new Address(Guid.NewGuid(), "https://exemplo.com/original", Dono)
            {
                NewUrl = "https://localhost:7000/abc123"
            };

            contexto.Addresses.Add(url);
            await contexto.SaveChangesAsync();

            return (servico, contexto, url.Id);
        }

        [Fact]
        public async Task DeleteUrl_DeOutroUsuario_LancaNotFound()
        {
            var (servico, _, id) = await ComUrlDoDono();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => servico.DeleteUrl(id, Invasor));
        }

        [Fact]
        public async Task DeleteUrl_DeOutroUsuario_NaoRemoveDoBanco()
        {
            var (servico, contexto, id) = await ComUrlDoDono();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => servico.DeleteUrl(id, Invasor));

            Assert.True(await contexto.Addresses.AnyAsync(a => a.Id == id));
        }

        [Fact]
        public async Task DeleteUrl_PeloDono_Remove()
        {
            var (servico, contexto, id) = await ComUrlDoDono();

            await servico.DeleteUrl(id, Dono);

            Assert.False(await contexto.Addresses.AnyAsync(a => a.Id == id));
        }

        [Fact]
        public async Task UpdateUrl_DeOutroUsuario_LancaNotFound()
        {
            var (servico, _, id) = await ComUrlDoDono();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => servico.UpdateUrl(id, Invasor, "https://sequestrado.com", null!));
        }

        [Fact]
        public async Task UpdateUrl_DeOutroUsuario_PreservaDestinoOriginal()
        {
            var (servico, contexto, id) = await ComUrlDoDono();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => servico.UpdateUrl(id, Invasor, "https://sequestrado.com", null!));

            var atual = await contexto.Addresses.AsNoTracking().FirstAsync(a => a.Id == id);
            Assert.Equal("https://exemplo.com/original", atual.OriginalUrl);
        }

        [Fact]
        public async Task UpdateUrl_PeloDono_AlteraDestino()
        {
            var (servico, contexto, id) = await ComUrlDoDono();

            await servico.UpdateUrl(id, Dono, "https://exemplo.com/novo", null!);

            var atual = await contexto.Addresses.AsNoTracking().FirstAsync(a => a.Id == id);
            Assert.Equal("https://exemplo.com/novo", atual.OriginalUrl);
        }

        [Fact]
        public async Task DeleteUrl_ComIdInexistente_LancaNotFound()
        {
            var (servico, _) = Montar();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => servico.DeleteUrl(Guid.NewGuid(), Dono));
        }
    }
}
