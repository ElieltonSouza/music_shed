using Dapper;
using music_shed.Infraestrutura;
using music_shed.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public class AgendaRepositorio : IAgendaRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        public AgendaRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InserirAsync(AgendaDisponivel agenda)
        {
            const string sql = @"
                INSERT INTO AgendasDisponiveis 
                (ProfessorId, DataHora, HoraFim, TipoAula, AlunosIds, Justificativa, LiberacaoExcepcional)
                VALUES (@ProfessorId, @DataHora, @HoraFim, @TipoAula, @AlunosIds, @Justificativa, @LiberacaoExcepcional);";

            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, agenda);
        }

        public async Task<IEnumerable<AgendaDisponivel>> ListarPorProfessorAsync(int professorId)
        {
            const string sql = @"
                SELECT *
                FROM AgendasDisponiveis
                WHERE ProfessorId = @ProfessorId
                ORDER BY DataHora;";

            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryAsync<AgendaDisponivel>(sql, new { ProfessorId = professorId });
        }

        public async Task<bool> ExisteAgendaNoMesmoHorarioAsync(int professorId, DateTime inicio, DateTime fim, int? ignorarAgendaId = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM AgendasDisponiveis
                WHERE ProfessorId = @ProfessorId
                  AND (DataHora < @Fim AND HoraFim > @Inicio)
                  AND (@IgnorarAgendaId IS NULL OR Id != @IgnorarAgendaId);";

            using IDbConnection con = _connectionFactory.CriarConexao();
            int count = await con.ExecuteScalarAsync<int>(sql, new
            {
                ProfessorId = professorId,
                Inicio = inicio,
                Fim = fim,
                IgnorarAgendaId = ignorarAgendaId
            });

            return count > 0;
        }

        public async Task<(bool Conflito, string NomeProfessor)> AlunoPossuiConflitoAsync(int alunoId, DateTime inicio, DateTime fim, int? ignorarAgendaId = null)
        {
            const string sql = @"
                SELECT TOP 1 u.NomeHash AS NomeProfessor
                FROM AgendasDisponiveis a
                JOIN Usuarios u ON a.ProfessorId = u.Id
                WHERE (a.DataHora < @Fim AND a.HoraFim > @Inicio)
                  AND (',' + a.AlunosIds + ',' LIKE '%,' + CONVERT(VARCHAR, @AlunoId) + ',%')
                  AND (@IgnorarAgendaId IS NULL OR a.Id != @IgnorarAgendaId);";

            using IDbConnection con = _connectionFactory.CriarConexao();
            var nome = await con.QueryFirstOrDefaultAsync<string>(sql, new
            {
                AlunoId = alunoId,
                Inicio = inicio,
                Fim = fim,
                IgnorarAgendaId = ignorarAgendaId
            });

            return (nome != null, nome ?? string.Empty);
        }

        public async Task<AgendaDisponivel?> ObterPorIdAsync(int id)
        {
            const string sql = "SELECT * FROM AgendasDisponiveis WHERE Id = @Id;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryFirstOrDefaultAsync<AgendaDisponivel>(sql, new { Id = id });
        }

        public async Task AtualizarAsync(AgendaDisponivel agenda)
        {
            const string sql = @"
                UPDATE AgendasDisponiveis
                SET DataHora = @DataHora,
                    HoraFim = @HoraFim,
                    TipoAula = @TipoAula,
                    AlunosIds = @AlunosIds,
                    Justificativa = @Justificativa,
                    LiberacaoExcepcional = @LiberacaoExcepcional
                WHERE Id = @Id;";

            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, agenda);
        }

        public async Task ExcluirAsync(int id)
        {
            using IDbConnection con = _connectionFactory.CriarConexao();

            const string excluirReagendamentos = "DELETE FROM SolicitacoesReagendamento WHERE AgendaId = @Id;";
            await con.ExecuteAsync(excluirReagendamentos, new { Id = id });

            const string excluirAgenda = "DELETE FROM AgendasDisponiveis WHERE Id = @Id;";
            await con.ExecuteAsync(excluirAgenda, new { Id = id });
        }

        public async Task<IEnumerable<AgendaDisponivel>> ListarTodasAsync()
        {
            const string sql = "SELECT * FROM AgendasDisponiveis ORDER BY DataHora;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryAsync<AgendaDisponivel>(sql);
        }

        public async Task<IEnumerable<AgendaDisponivel>> ListarAgendasDoAlunoAsync(int alunoId)
        {
            const string sql = @"
                SELECT a.*, u.NomeHash AS NomeProfessor
                FROM AgendasDisponiveis a
                JOIN Usuarios u ON u.Id = a.ProfessorId
                WHERE ISNULL(a.AlunosIds, '') <> ''
                  AND ',' + a.AlunosIds + ',' LIKE '%,' + CAST(@AlunoId AS VARCHAR) + ',%'
                ORDER BY a.DataHora;";

            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryAsync<AgendaDisponivel, string, AgendaDisponivel>(
                sql,
                (agenda, nomeProfessor) =>
                {
                    agenda.NomeProfessor = nomeProfessor;
                    return agenda;
                },
                new { AlunoId = alunoId },
                splitOn: "NomeProfessor"
            );
        }
    }
}