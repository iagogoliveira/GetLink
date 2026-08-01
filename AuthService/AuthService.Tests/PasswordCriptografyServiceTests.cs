using AuthService.Services;

namespace AuthService.Tests
{
    public class PasswordCriptografyServiceTests
    {
        [Fact]
        public void ValidPassword_ComSenhaCorreta_RetornaTrue()
        {
            var hash = PasswordCriptografyService.GeneratePasswordHash("SenhaCorreta123");

            Assert.True(PasswordCriptografyService.ValidPassword("SenhaCorreta123", hash));
        }

        [Fact]
        public void ValidPassword_ComSenhaErrada_RetornaFalse()
        {
            var hash = PasswordCriptografyService.GeneratePasswordHash("SenhaCorreta123");

            Assert.False(PasswordCriptografyService.ValidPassword("SenhaErrada", hash));
        }

        // Salt aleatorio: duas contas com a mesma senha nao podem produzir o
        // mesmo hash, senao um vazamento do banco revelaria senhas repetidas.
        [Fact]
        public void GeneratePasswordHash_ComMesmaSenha_ProduzHashesDiferentes()
        {
            var primeiro = PasswordCriptografyService.GeneratePasswordHash("MesmaSenha");
            var segundo = PasswordCriptografyService.GeneratePasswordHash("MesmaSenha");

            Assert.NotEqual(primeiro, segundo);
        }

        [Theory]
        [InlineData("nao-e-base64!!")]
        [InlineData("")]
        public void ValidPassword_ComHashCorrompido_RetornaFalseSemLancar(string hash)
        {
            Assert.False(PasswordCriptografyService.ValidPassword("qualquer", hash));
        }

        // 48 bytes era o formato anterior ao versionamento, sem cabecalho.
        [Fact]
        public void ValidPassword_ComFormatoAntigo_RetornaFalse()
        {
            var formatoLegado = Convert.ToBase64String(new byte[48]);

            Assert.False(PasswordCriptografyService.ValidPassword("qualquer", formatoLegado));
        }

        [Fact]
        public void PrecisaAtualizar_ComHashRecemGerado_RetornaFalse()
        {
            var hash = PasswordCriptografyService.GeneratePasswordHash("Senha");

            Assert.False(PasswordCriptografyService.PrecisaAtualizar(hash));
        }
    }
}
