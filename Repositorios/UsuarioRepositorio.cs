using Dapper;
using music_shed.Infraestrutura;
using music_shed.Models;
using System.Data;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    /// <summary>
    /// Implementação do repositório de usuários utilizando Dapper.
    /// </summary>
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// Construtor com injeção da fábrica de conexões.
        /// </summary>
        /// <param name="connectionFactory">Instância da fábrica de conexões com o banco.</param>
        public UsuarioRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <inheritdoc/>
        public async Task InserirAsync(Usuario usuario)
        {
            const string sql = @"
                INSERT INTO Usuarios (NomeHash, EmailHash, DataCadastro)
                VALUES (@NomeHash, @EmailHash, @DataCadastro);
            ";

            using IDbConnection conexao = _connectionFactory.CriarConexao();
            await conexao.ExecuteAsync(sql, usuario);
        }

        /// <inheritdoc/>
        public async Task<bool> UsuarioJaExisteAsync(string emailHash)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Usuarios
                WHERE EmailHash = @EmailHash;
            ";

            using IDbConnection conexao = _connectionFactory.CriarConexao();
            int count = await conexao.ExecuteScalarAsync<int>(sql, new { EmailHash = emailHash });
            return count > 0;
        }
    }
}