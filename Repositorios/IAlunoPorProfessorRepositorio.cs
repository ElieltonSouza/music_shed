using music_shed.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public interface IAlunoPorProfessorRepositorio
    {
        Task<List<int>> ObterAlunosPorProfessorAsync(int professorId);
        Task InserirAsync(int professorId, List<int> alunosIds);
        Task RemoverTodosAsync(int professorId);
    }
}