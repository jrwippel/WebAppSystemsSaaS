using System.Security.Cryptography;

namespace WebAppSystems.Helper
{
    public static class Criptografia
    {
        private const int Iterations = 100_000;
        private const int HashSize = 32; // 256 bits
        private const int SaltSize = 16; // 128 bits

        /// <summary>
        /// Gera hash PBKDF2 com salt aleatório. Formato: "salt:hash" em Base64.
        /// </summary>
        public static string GerarHash(this string valor)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(valor, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifica senha contra hash PBKDF2. Suporta hashes SHA-1 legados para migração.
        /// </summary>
        public static bool VerificarHash(this string valor, string hashArmazenado)
        {
            // Suporte a hashes SHA-1 legados (formato hex sem ":")
            if (!hashArmazenado.Contains(':'))
                return VerificarHashLegado(valor, hashArmazenado);

            var partes = hashArmazenado.Split(':');
            if (partes.Length != 2) return false;

            var salt = Convert.FromBase64String(partes[0]);
            var hashEsperado = Convert.FromBase64String(partes[1]);
            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(valor, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }

        private static bool VerificarHashLegado(string valor, string hashSha1)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytes = System.Text.Encoding.ASCII.GetBytes(valor);
            var hash = sha1.ComputeHash(bytes);
            var hex = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return hex == hashSha1;
        }
    }
}
