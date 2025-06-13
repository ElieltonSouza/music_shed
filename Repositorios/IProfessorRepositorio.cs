using music_shed.Models;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    /// <summary>
    /// Interface para operações de persistência de dados de professores.
    /// </summary>
    public interface IProfessorRepositorio
    {
        /// <summary>
        /// Insere um novo professor no banco de dados.
        /// </summary>
        /// <param name="professor">Objeto Professor a ser cadastrado.</param>
        Task CadastrarAsync(Professor professor);
    }
}