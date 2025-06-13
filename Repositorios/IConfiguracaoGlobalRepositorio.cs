using System.Threading.Tasks;
using music_shed.Models;

namespace music_shed.Repositorios
{
    public interface IConfiguracaoGlobalRepositorio
    {
        Task<int> ObterTempoConfirmacaoPresencaMinutosAsync();
        Task<int> ObterMinutosAntecedenciaCancelamentoAlunoAsync();
        Task<int> ObterMinutosAntecedenciaReagendamentoAlunoAsync();
        Task<ConfiguracaoGlobal> ObterAsync();
    }
}