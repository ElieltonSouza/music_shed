using Dapper;
using music_shed.Infraestrutura;
using music_shed.Models;
using System.Threading.Tasks;

namespace music_shed.Repositorios
{
    /// <summary>
    /// Implementação do repositório de professores utilizando Dapper.
    /// </summary>
    public class ProfessorRepositorio : IProfessorRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// Construtor com injeção de dependência da fábrica de conexão.
        /// </summary>
        public ProfessorRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Cadastra um novo professor no banco de dados.
        /// </summary>
        /// <param name="professor">Professor a ser salvo.</param>
        public async Task CadastrarAsync(Professor professor)
        {
            var sql = @"
                INSERT INTO Professores (Nome, Email, DataCadastro)
                VALUES (@Nome, @Email, @DataCadastro);";

            using var conexao = _connectionFactory.CriarConexao();
            await conexao.ExecuteAsync(sql, professor);
        }
    }
}