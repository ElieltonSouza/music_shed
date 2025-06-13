using music_shed.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public interface ICancelamentoAulaRepositorio
    {
        Task InserirAsync(CancelamentoAula cancelamento);
        Task<List<CancelamentoAula>> ListarPorProfessorEMesAsync(int professorId, int mes, int ano);
    }
}