using System.Buffers.Binary;
using System.Security.Cryptography;

namespace AuthService.Services
{
    public class PasswordCriptografyService
    {
        private const int TamanhoSalt = 16;
        private const int TamanhoHash = 32;

        // Recomendacao OWASP para PBKDF2-HMAC-SHA256. Aumentar com o tempo.
        private const int IteracoesAtuais = 600_000;

        private const byte VersaoAtual = 1;
        private const int TamanhoCabecalho = 1 + 4; // versao + iteracoes
        private const int TamanhoFormato = TamanhoCabecalho + TamanhoSalt + TamanhoHash;

        // Hash descartavel, gerado uma vez por processo. Ver SimulateValidation.
        private static readonly Lazy<string> HashDescartavel =
            new(() => GeneratePasswordHash(Guid.NewGuid().ToString()));

        public static string GeneratePasswordHash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
            byte[] hash = Derivar(password, salt, IteracoesAtuais);

            // Layout: [versao:1][iteracoes:4][salt:16][hash:32]
            // Guardar as iteracoes permite aumentar o custo no futuro sem
            // invalidar as senhas ja cadastradas.
            var resultado = new byte[TamanhoFormato];
            resultado[0] = VersaoAtual;
            BinaryPrimitives.WriteInt32BigEndian(resultado.AsSpan(1, 4), IteracoesAtuais);
            salt.CopyTo(resultado.AsSpan(TamanhoCabecalho));
            hash.CopyTo(resultado.AsSpan(TamanhoCabecalho + TamanhoSalt));

            return Convert.ToBase64String(resultado);
        }

        public static bool ValidPassword(string password, string storedHash)
        {
            if (!TentarLer(storedHash, out int iteracoes, out byte[] salt, out byte[] hashEsperado))
            {
                return false;
            }

            byte[] hashCalculado = Derivar(password, salt, iteracoes);

            // Comparacao em tempo constante: sair no primeiro byte diferente
            // vazaria informacao sobre o hash por medicao de tempo.
            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }

        // Gasta aproximadamente o mesmo tempo de uma validacao real. Usado quando o
        // login nao existe, para que a resposta nao denuncie quais logins estao cadastrados.
        public static void SimulateValidation(string password)
        {
            ValidPassword(password, HashDescartavel.Value);
        }

        // Indica que o hash foi gerado com um custo menor que o atual e merece ser
        // regravado no proximo login bem-sucedido.
        public static bool PrecisaAtualizar(string storedHash)
        {
            return TentarLer(storedHash, out int iteracoes, out _, out _)
                   && iteracoes < IteracoesAtuais;
        }

        private static bool TentarLer(string storedHash, out int iteracoes, out byte[] salt, out byte[] hash)
        {
            iteracoes = 0;
            salt = [];
            hash = [];

            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String(storedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            if (bytes.Length != TamanhoFormato || bytes[0] != VersaoAtual)
            {
                return false;
            }

            iteracoes = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(1, 4));

            if (iteracoes <= 0)
            {
                return false;
            }

            salt = bytes[TamanhoCabecalho..(TamanhoCabecalho + TamanhoSalt)];
            hash = bytes[(TamanhoCabecalho + TamanhoSalt)..];

            return true;
        }

        private static byte[] Derivar(string password, byte[] salt, int iteracoes)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iteracoes,
                HashAlgorithmName.SHA256,
                TamanhoHash);
        }
    }
}
