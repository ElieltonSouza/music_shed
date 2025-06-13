using music_shed.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public interface IAgendaRepositorio
    {
        Task InserirAsync(AgendaDisponivel agenda);
        Task<IEnumerable<AgendaDisponivel>> ListarPorProfessorAsync(int professorId);
        Task<AgendaDisponivel?> ObterPorIdAsync(int id);
        Task AtualizarAsync(AgendaDisponivel agenda);
        Task ExcluirAsync(int id);
        Task<bool> ExisteAgendaNoMesmoHorarioAsync(int professorId, DateTime inicio, DateTime fim, int? ignorarAgendaId = null);
        Task<(bool Conflito, string NomeProfessor)> AlunoPossuiConflitoAsync(int alunoId, DateTime inicio, DateTime fim, int? ignorarAgendaId = null);
        Task<IEnumerable<AgendaDisponivel>> ListarTodasAsync();
        Task<IEnumerable<AgendaDisponivel>> ListarAgendasDoAlunoAsync(int alunoId);
    }
}