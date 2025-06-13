using System.Security.Cryptography;
using System.Text;

namespace music_shed.Servicos
{
    /// <summary>
    /// Serviço responsável por aplicar criptografia de hash (SHA256) para anonimização de dados pessoais, conforme a LGPD.
    /// </summary>
    public class ServicoDeHash
    {
        /// <summary>
        /// Gera um hash SHA256 de uma string de entrada.
        /// Este método é utilizado para anonimização de dados pessoais sensíveis.
        /// </summary>
        /// <param name="valor">Valor original a ser anonimizado.</param>
        /// <returns>Representação criptografada (hash em Base64) do valor.</returns>
        public string GerarHash(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(valor.Trim());
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}