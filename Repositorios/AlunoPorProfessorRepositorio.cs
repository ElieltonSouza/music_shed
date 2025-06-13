using Dapper;
using music_shed.Infraestrutura;
using music_shed.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public class AlunoPorProfessorRepositorio : IAlunoPorProfessorRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        public AlunoPorProfessorRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<int>> ObterAlunosPorProfessorAsync(int professorId)
        {
            const string sql = "SELECT AlunoId FROM AlunosPorProfessor WHERE ProfessorId = @ProfessorId;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            var alunos = await con.QueryAsync<int>(sql, new { ProfessorId = professorId });
            return alunos.AsList();
        }

        public async Task InserirAsync(int professorId, List<int> alunosIds)
        {
            const string sql = @"
                INSERT INTO AlunosPorProfessor (ProfessorId, AlunoId)
                VALUES (@ProfessorId, @AlunoId);";

            using IDbConnection con = _connectionFactory.CriarConexao();
            foreach (var alunoId in alunosIds)
            {
                await con.ExecuteAsync(sql, new { ProfessorId = professorId, AlunoId = alunoId });
            }
        }

        public async Task RemoverTodosAsync(int professorId)
        {
            const string sql = "DELETE FROM AlunosPorProfessor WHERE ProfessorId = @ProfessorId;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, new { ProfessorId = professorId });
        }
    }
}