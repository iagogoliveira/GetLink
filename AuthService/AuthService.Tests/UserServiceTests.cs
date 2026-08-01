using System.Diagnostics;
using AuthService.Data;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Tests
{
    public class UserServiceTests
    {
        private static UserService Montar()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new UserService(new UserRepository(new AppDbContext(options)));
        }

        private static async Task<UserService> ComUsuario(
            string login = "iago",
            string senha = "MinhaSenha!2026")
        {
            var servico = Montar();
            await servico.CreateUserAsync(new User("Iago", login, senha, $"{login}@exemplo.com"));
            return servico;
        }

        [Fact]
        public async Task AuthenticateUserAsync_ComCredenciaisCorretas_RetornaUsuario()
        {
            var servico = await ComUsuario();

            var usuario = await servico.AuthenticateUserAsync("iago", "MinhaSenha!2026");

            Assert.NotNull(usuario);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ComSenhaErrada_RetornaNull()
        {
            var servico = await ComUsuario();

            Assert.Null(await servico.AuthenticateUserAsync("iago", "senhaErrada"));
        }

        [Fact]
        public async Task AuthenticateUserAsync_ComLoginInexistente_RetornaNull()
        {
            var servico = await ComUsuario();

            Assert.Null(await servico.AuthenticateUserAsync("ninguem", "qualquer"));
        }

        [Fact]
        public async Task AuthenticateUserAsync_ComUsuarioInativo_RetornaNull()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var contexto = new AppDbContext(options);
            var servico = new UserService(new UserRepository(contexto));

            await servico.CreateUserAsync(new User("Ana", "ana", "SenhaDaAna!1", "ana@exemplo.com"));

            var usuario = await contexto.Users.FirstAsync();
            usuario.Active = false;
            await contexto.SaveChangesAsync();

            Assert.Null(await servico.AuthenticateUserAsync("ana", "SenhaDaAna!1"));
        }

        [Fact]
        public async Task CreateUserAsync_ComLoginJaExistente_LancaInvalidOperation()
        {
            var servico = await ComUsuario();

            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
                () => servico.CreateUserAsync(new User("Outro", "iago", "OutraSenha!1", "outro@exemplo.com")));

            Assert.Equal("Login already in use.", excecao.Message);
        }

        // Se o hash so fosse calculado quando o login existe, a diferenca de tempo
        // entre as duas respostas revelaria quais logins estao cadastrados.
        [Fact]
        public async Task AuthenticateUserAsync_LoginInexistente_LevaTempoComparavelAoExistente()
        {
            var servico = await ComUsuario("ana", "SenhaDaAna!1");

            async Task<double> Medir(string login, string senha)
            {
                await servico.AuthenticateUserAsync(login, senha); // aquece

                var relogio = Stopwatch.StartNew();
                for (var i = 0; i < 3; i++)
                {
                    await servico.AuthenticateUserAsync(login, senha);
                }

                return relogio.Elapsed.TotalMilliseconds / 3;
            }

            var existente = await Medir("ana", "senhaErrada");
            var inexistente = await Medir("naoexiste", "senhaErrada");
            var razao = inexistente / existente;

            Assert.InRange(razao, 0.5, 2.0);
        }
    }
}
