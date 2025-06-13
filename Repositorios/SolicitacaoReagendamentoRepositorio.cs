using Dapper;
using music_shed.Models;
using music_shed.Infraestrutura;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace music_shed.Repositorios
{
    public class SolicitacaoReagendamentoRepositorio : ISolicitacaoReagendamentoRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        public SolicitacaoReagendamentoRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InserirAsync(SolicitacaoReagendamento solicitacao)
        {
            const string sql = @"
                INSERT INTO SolicitacoesReagendamento
                    (AgendaId, AlunoId, Justificativa, DataSolicitacao, Status, ObservacaoResposta)
                VALUES
                    (@AgendaId, @AlunoId, @Justificativa, @DataSolicitacao, @Status, @ObservacaoResposta);";

            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, solicitacao);
        }

        public async Task<List<SolicitacaoReagendamento>> ListarPorAlunoAsync(int alunoId)
        {
            const string sql = @"
                SELECT * FROM SolicitacoesReagendamento
                WHERE AlunoId = @AlunoId
                ORDER BY DataSolicitacao DESC";

            using IDbConnection con = _connectionFactory.CriarConexao();
            var result = await con.QueryAsync<SolicitacaoReagendamento>(sql, new { AlunoId = alunoId });
            return result.AsList();
        }

        public async Task<List<SolicitacaoReagendamento>> ListarPendentesDoProfessorAsync(int professorId)
        {
            const string sql = @"
                SELECT 
                    r.*, 
                    a.*, 
                    u.*
                FROM SolicitacoesReagendamento r
                INNER JOIN AgendasDisponiveis a ON r.AgendaId = a.Id
                INNER JOIN Usuarios u ON r.AlunoId = u.Id
                WHERE a.ProfessorId = @ProfessorId AND r.Status = 'Pendente'
                ORDER BY r.DataSolicitacao DESC";

            using IDbConnection con = _connectionFactory.CriarConexao();

            var resultado = await con.QueryAsync<SolicitacaoReagendamento, AgendaDisponivel, Usuario, SolicitacaoReagendamento>(
                sql,
                (solicitacao, agenda, aluno) =>
                {
                    solicitacao.Agenda = agenda;
                    solicitacao.Aluno = aluno;
                    return solicitacao;
                },
                new { ProfessorId = professorId },
                splitOn: "Id,Id"
            );

            return resultado.ToList();
        }

        public async Task<SolicitacaoReagendamento?> ObterPorIdAsync(int id)
        {
            const string sql = "SELECT * FROM SolicitacoesReagendamento WHERE Id = @Id";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryFirstOrDefaultAsync<SolicitacaoReagendamento>(sql, new { Id = id });
        }

        public async Task AtualizarStatusAsync(int id, string novoStatus, string? observacao)
        {
            const string sql = @"
                UPDATE SolicitacoesReagendamento
                SET Status = @Status,
                    ObservacaoResposta = @Observacao
                WHERE Id = @Id";

            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, new { Id = id, Status = novoStatus, Observacao = observacao });
        }

        // ✅ NOVO MÉTODO
        public async Task<bool> ExisteSolicitacaoValidaAsync(int agendaId, int alunoId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM SolicitacoesReagendamento
                WHERE AgendaId = @agendaId
                AND AlunoId = @alunoId
                AND Status IN ('Pendente', 'Aprovado')";

            using IDbConnection con = _connectionFactory.CriarConexao();
            var count = await con.ExecuteScalarAsync<int>(sql, new { agendaId, alunoId });
            return count > 0;
        }
    }
}