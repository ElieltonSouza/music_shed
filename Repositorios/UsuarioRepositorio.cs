using Dapper;
using music_shed.Infraestrutura;
using music_shed.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        public UsuarioRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InserirAsync(Usuario usuario)
        {
            const string sql = @"
                INSERT INTO Usuarios (NomeHash, EmailHash, Perfil, DataCadastro)
                VALUES (@NomeHash, @EmailHash, @Perfil, @DataCadastro);";
            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, usuario);
        }

        public async Task<bool> UsuarioJaExisteAsync(string emailHash)
        {
            const string sql = "SELECT COUNT(1) FROM Usuarios WHERE EmailHash = @EmailHash;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.ExecuteScalarAsync<int>(sql, new { EmailHash = emailHash }) > 0;
        }

        public async Task<Usuario?> ObterPorEmailHashAsync(string emailHash)
        {
            const string sql = "SELECT TOP 1 * FROM Usuarios WHERE EmailHash = @EmailHash;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryFirstOrDefaultAsync<Usuario>(sql, new { EmailHash = emailHash });
        }

        public async Task CriarAsync(Usuario usuario, bool incluiSenha = false)
        {
            string sql = incluiSenha
                ? @"INSERT INTO Usuarios (NomeHash, EmailHash, SenhaHash, Perfil)
                   VALUES (@NomeHash, @EmailHash, @SenhaHash, @Perfil);"
                : @"INSERT INTO Usuarios (NomeHash, EmailHash, Perfil)
                   VALUES (@NomeHash, @EmailHash, @Perfil);";

            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, usuario);
        }

        /* ---------- Listagem paginada ---------- */
        public async Task<IEnumerable<Usuario>> ListarPaginadoAsync(int skip, int take)
        {
            const string sql = @"
                SELECT * FROM Usuarios
                ORDER BY Id
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryAsync<Usuario>(sql, new { Skip = skip, Take = take });
        }

        public async Task<int> ContarAsync()
        {
            const string sql = "SELECT COUNT(1) FROM Usuarios;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.ExecuteScalarAsync<int>(sql);
        }

        /* ---------- Edição ---------- */
        public async Task<Usuario?> ObterPorIdAsync(int id)
        {
            const string sql = "SELECT * FROM Usuarios WHERE Id = @Id;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
        }

        public async Task AtualizarNomeAsync(int id, string novoNome)
        {
            const string sql = "UPDATE Usuarios SET NomeHash = @Nome WHERE Id = @Id;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, new { Id = id, Nome = novoNome });
        }
        public async Task ExcluirAsync(int id)
        {
            const string sql = "DELETE FROM Usuarios WHERE Id = @Id;";
            using var con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, new { Id = id });
        }
        public async Task MarcarNomeEditadoManualmenteAsync(int usuarioId)
        {
            const string sql = "UPDATE Usuarios SET NomeEditadoManualmente = 1 WHERE Id = @Id;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            await con.ExecuteAsync(sql, new { Id = usuarioId });
        }
        public async Task<IEnumerable<Usuario>> ListarPorPerfilAsync(string perfil)
        {
            const string sql = "SELECT * FROM Usuarios WHERE Perfil = @Perfil ORDER BY NomeHash;";
            using IDbConnection con = _connectionFactory.CriarConexao();
            return await con.QueryAsync<Usuario>(sql, new { Perfil = perfil });
        }
        public async Task<List<Usuario>> ListarAlunosAssociadosAsync(int professorId)
        {
            const string sql = @"
                SELECT u.*
                FROM Usuarios u
                INNER JOIN AlunosPorProfessor ap ON u.Id = ap.AlunoId
                WHERE ap.ProfessorId = @ProfessorId
                ORDER BY u.NomeHash;";

            using IDbConnection con = _connectionFactory.CriarConexao();
            var alunos = await con.QueryAsync<Usuario>(sql, new { ProfessorId = professorId });
            return alunos.ToList();
        }
    }
}