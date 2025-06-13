using Microsoft.AspNetCore.Mvc;
using music_shed.Models;
using music_shed.Repositorios;
using System.Threading.Tasks;
using System;
using Dapper;
using music_shed.Infraestrutura;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace music_shed.Controllers
{
    public class ConfiguracoesGlobaisController : Controller
    {
        private readonly IConnectionFactory _connectionFactory;

        public ConfiguracoesGlobaisController(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            if (perfil != "Administrador")
                return RedirectToAction("AccessDenied", "Logins");

            using IDbConnection connection = _connectionFactory.CriarConexao();

            string sql = "SELECT TOP 1 * FROM ConfiguracoesGlobais ORDER BY Id DESC";
            var config = await connection.QueryFirstOrDefaultAsync<ConfiguracaoGlobal>(sql);

            if (config == null)
            {
                config = new ConfiguracaoGlobal
                {
                    TempoConfirmacaoPresencaMinutos = 1440,
                    MinutosAntecedenciaCancelamentoAluno = 60,
                    MinutosAntecedenciaReagendamentoAluno = 60
                };
            }

            return View(config);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(ConfiguracaoGlobal config)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            if (perfil != "Administrador")
                return RedirectToAction("AccessDenied", "Logins");

            if (!ModelState.IsValid)
                return View(config);

            config.CriadoEm = DateTime.Now;

            using IDbConnection connection = _connectionFactory.CriarConexao();

            string insert = @"
                INSERT INTO ConfiguracoesGlobais
                (TempoConfirmacaoPresencaMinutos, MinutosAntecedenciaCancelamentoAluno, MinutosAntecedenciaReagendamentoAluno, CriadoEm)
                VALUES (@TempoConfirmacaoPresencaMinutos, @MinutosAntecedenciaCancelamentoAluno, @MinutosAntecedenciaReagendamentoAluno, @CriadoEm)";

            await connection.ExecuteAsync(insert, config);

            TempData["MensagemSucesso"] = "Configurações salvas com sucesso.";
            return RedirectToAction("Editar");
        }
    }
}