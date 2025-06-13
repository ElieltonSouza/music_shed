using music_shed.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public interface ISolicitacaoReagendamentoRepositorio
    {
        /// <summary>
        /// Insere uma nova solicitação de reagendamento.
        /// </summary>
        Task InserirAsync(SolicitacaoReagendamento solicitacao);

        /// <summary>
        /// Retorna todas as solicitações feitas por um aluno específico.
        /// </summary>
        Task<List<SolicitacaoReagendamento>> ListarPorAlunoAsync(int alunoId);

        /// <summary>
        /// Retorna todas as solicitações pendentes de reagendamento para um professor,
        /// incluindo as informações da agenda e do aluno.
        /// </summary>
        Task<List<SolicitacaoReagendamento>> ListarPendentesDoProfessorAsync(int professorId);

        /// <summary>
        /// Retorna uma solicitação específica pelo ID.
        /// </summary>
        Task<SolicitacaoReagendamento?> ObterPorIdAsync(int id);

        /// <summary>
        /// Atualiza o status e a observação de uma solicitação.
        /// </summary>
        Task AtualizarStatusAsync(int id, string novoStatus, string? observacao);

        /// <summary>
        /// Verifica se já existe uma solicitação válida (pendente ou aprovada) para a mesma agenda e aluno.
        /// </summary>
        Task<bool> ExisteSolicitacaoValidaAsync(int agendaId, int alunoId);
    }
}