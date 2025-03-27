using music_shed.Models;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    /// <summary>
    /// Contrato para operações de persistência de usuários no banco de dados.
    /// </summary>
    public interface IUsuarioRepositorio
    {
        /// <summary>
        /// Insere um novo usuário no banco de dados.
        /// </summary>
        /// <param name="usuario">Objeto contendo os dados do usuário anonimizados.</param>
        /// <returns>Task assíncrona.</returns>
        Task InserirAsync(Usuario usuario);

        /// <summary>
        /// Verifica se um usuário já está cadastrado com base no e-mail anonimizado (hash).
        /// </summary>
        /// <param name="emailHash">Hash do e-mail.</param>
        /// <returns>Verdadeiro se existir, falso caso contrário.</returns>
        Task<bool> UsuarioJaExisteAsync(string emailHash);
    }
}