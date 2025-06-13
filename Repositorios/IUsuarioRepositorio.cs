using music_shed.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    /// <summary>Contrato para operações de persistência de usuários.</summary>
    public interface IUsuarioRepositorio
    {
        Task InserirAsync(Usuario usuario);
        Task<bool> UsuarioJaExisteAsync(string emailHash);
        Task<Usuario?> ObterPorEmailHashAsync(string emailHash);
        Task CriarAsync(Usuario usuario, bool incluiSenha = false);
        Task<IEnumerable<Usuario>> ListarPaginadoAsync(int skip, int take);
        Task<int> ContarAsync();
        Task<Usuario?> ObterPorIdAsync(int id);
        Task AtualizarNomeAsync(int id, string novoNome);
        Task ExcluirAsync(int id);
        Task MarcarNomeEditadoManualmenteAsync(int usuarioId);
        Task<IEnumerable<Usuario>> ListarPorPerfilAsync(string perfil);
        Task<List<Usuario>> ListarAlunosAssociadosAsync(int professorId);
    }
}