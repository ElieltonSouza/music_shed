using Dapper;
using music_shed.Models;
using music_shed.Infraestrutura;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public class CancelamentoAulaRepositorio : ICancelamentoAulaRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        public CancelamentoAulaRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InserirAsync(CancelamentoAula cancelamento)
        {
            using IDbConnection con = _connectionFactory.CriarConexao(); // <- AQUI
            string sql = @"
                INSERT INTO CancelamentosAula (AgendaId, AlunoId, Justificativa)
                VALUES (@AgendaId, @AlunoId, @Justificativa);";

            await con.ExecuteAsync(sql, cancelamento);
        }

        public async Task<List<CancelamentoAula>> ListarPorProfessorEMesAsync(int professorId, int mes, int ano)
        {
            using IDbConnection con = _connectionFactory.CriarConexao(); // <- E AQUI

            string sql = @"
                SELECT c.*, u.Nome AS NomeAluno, a.DataHora
                FROM CancelamentosAula c
                INNER JOIN Usuarios u ON c.AlunoId = u.Id
                INNER JOIN AgendasDisponiveis a ON c.AgendaId = a.Id
                WHERE MONTH(c.DataCancelamento) = @Mes
                  AND YEAR(c.DataCancelamento) = @Ano
                  AND a.ProfessorId = @ProfessorId
                ORDER BY c.DataCancelamento DESC";

            var lookup = new Dictionary<int, CancelamentoAula>();

            var resultado = await con.QueryAsync<CancelamentoAula, Usuario, AgendaDisponivel, CancelamentoAula>(
                sql,
                (cancelamento, aluno, agenda) =>
                {
                    if (!lookup.TryGetValue(cancelamento.Id, out var c))
                    {
                        cancelamento.Aluno = aluno;
                        cancelamento.Agenda = agenda;
                        lookup.Add(cancelamento.Id, cancelamento);
                    }
                    return cancelamento;
                },
                new { Mes = mes, Ano = ano, ProfessorId = professorId }
            );

            return resultado.ToList();
        }
    }
}