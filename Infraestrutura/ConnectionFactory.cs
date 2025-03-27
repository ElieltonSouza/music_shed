using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace music_shed.Infraestrutura
{
    /// <summary>
    /// Implementação da fábrica de conexões com SQL Server.
    /// </summary>
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly IConfiguration _configuracao;

        public ConnectionFactory(IConfiguration configuracao)
        {
            _configuracao = configuracao;
        }

        /// <summary>
        /// Cria e retorna uma conexão com o banco de dados SQL Server, usando a string de conexão do appsettings.json.
        /// </summary>
        /// <returns>IDbConnection aberta e pronta para uso.</returns>
        public IDbConnection CriarConexao()
        {
            var connectionString = _configuracao.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        }
    }
}