using System.Data;

namespace music_shed.Infraestrutura
{
    /// <summary>
    /// Interface para criação de conexões com o banco de dados.
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// Cria e retorna uma nova conexão com o banco de dados.
        /// </summary>
        /// <returns>Instância de IDbConnection pronta para uso.</returns>
        IDbConnection CriarConexao();
    }
}