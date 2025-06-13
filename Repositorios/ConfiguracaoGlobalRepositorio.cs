using Dapper;
using music_shed.Infraestrutura;
using System.Data;
using System.Threading.Tasks;
using music_shed.Models;

namespace music_shed.Repositorios
{
    public class ConfiguracaoGlobalRepositorio : IConfiguracaoGlobalRepositorio
    {
        private readonly IConnectionFactory _connectionFactory;

        public ConfiguracaoGlobalRepositorio(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> ObterTempoConfirmacaoPresencaMinutosAsync()
        {
            using IDbConnection connection = _connectionFactory.CriarConexao();
            string sql = "SELECT TOP 1 TempoConfirmacaoPresencaMinutos FROM ConfiguracoesGlobais ORDER BY Id DESC";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<int> ObterMinutosAntecedenciaCancelamentoAlunoAsync()
        {
            using IDbConnection connection = _connectionFactory.CriarConexao();
            string sql = "SELECT TOP 1 MinutosAntecedenciaCancelamentoAluno FROM ConfiguracoesGlobais ORDER BY Id DESC";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<int> ObterMinutosAntecedenciaReagendamentoAlunoAsync()
        {
            using IDbConnection connection = _connectionFactory.CriarConexao();
            string sql = "SELECT TOP 1 MinutosAntecedenciaReagendamentoAluno FROM ConfiguracoesGlobais ORDER BY Id DESC";
            return await connection.ExecuteScalarAsync<int>(sql);
        }
        public async Task<ConfiguracaoGlobal> ObterAsync()
        {
            using IDbConnection con = _connectionFactory.CriarConexao();
            string sql = "SELECT TOP 1 * FROM ConfiguracoesGlobais ORDER BY Id DESC;";
            return await con.QueryFirstOrDefaultAsync<ConfiguracaoGlobal>(sql);
        }

    }
}