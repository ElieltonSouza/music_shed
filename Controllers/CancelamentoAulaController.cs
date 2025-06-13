using Microsoft.AspNetCore.Mvc;
using music_shed.Models;
using music_shed.Repositorios;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Data;
using Dapper;
using music_shed.Infraestrutura;
using Microsoft.AspNetCore.Authorization;

namespace music_shed.Controllers
{
    public class CancelamentoAulaController : Controller
    {
        private readonly ICancelamentoAulaRepositorio _cancelamentoRepo;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfiguracaoGlobalRepositorio _configGlobalRepo;

        public CancelamentoAulaController(ICancelamentoAulaRepositorio cancelamentoRepo,
                                          IConnectionFactory connectionFactory,
                                          IConfiguracaoGlobalRepositorio configGlobalRepo)
        {
            _cancelamentoRepo = cancelamentoRepo;
            _connectionFactory = connectionFactory;
            _configGlobalRepo = configGlobalRepo;
        }

        [HttpGet]
        public IActionResult Confirmar(int agendaId)
        {
            ViewBag.AgendaId = agendaId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirmar(int agendaId, string justificativa)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var perfil = HttpContext.Session.GetString("Perfil");

                if (usuarioId == null || string.IsNullOrEmpty(perfil))
                {
                    TempData["MensagemErro"] = "Sessão expirada. Faça login novamente.";
                    return RedirectToAction("Login", "Autenticacao");
                }

                var agendaRepo = new AgendaRepositorio(_connectionFactory);
                var agenda = await agendaRepo.ObterPorIdAsync(agendaId);
                if (agenda == null) return NotFound();

                if (perfil == "Aluno")
                {
                    int minutosMinimos = agenda.MinutosAntecedenciaCancelamentoAluno ?? 60;
                    var limiteMinimo = agenda.DataHora.AddMinutes(-minutosMinimos);

                    if (DateTime.Now > limiteMinimo)
                    {
                        TempData["MensagemErro"] = $"Você só pode cancelar com no mínimo {minutosMinimos} minutos de antecedência.";
                        return RedirectToAction("MinhasAulas", "AgendasDisponiveis");
                    }
                }
                else if (perfil == "Professor")
                {
                    // ✅ Obtém o tempo limite para cancelamento pós-horário da aula
                    var config = await _configGlobalRepo.ObterAsync();
                    int minutosLimite = config.MinutosAntecedenciaCancelamentoAluno;
                    var fimLimite = agenda.DataHora.AddMinutes(minutosLimite);

                    if (DateTime.Now > fimLimite)
                    {
                        TempData["MensagemErro"] = $"O prazo para cancelar a aula expirou ({minutosLimite} minutos após o início).";
                        return RedirectToAction("Index", "AgendasDisponiveis");
                    }
                }

                var cancelamento = new CancelamentoAula
                {
                    AgendaId = agendaId,
                    AlunoId = usuarioId.Value,
                    Justificativa = justificativa,
                    DataCancelamento = DateTime.Now
                };

                await _cancelamentoRepo.InserirAsync(cancelamento);

                string status = perfil == "Professor" ? "CanceladaPeloProfessor" : "CanceladaPeloAluno";

                using IDbConnection con = _connectionFactory.CriarConexao();
                string sqlUpdate = "UPDATE AgendasDisponiveis SET StatusAula = @Status WHERE Id = @Id";
                await con.ExecuteAsync(sqlUpdate, new { Status = status, Id = agendaId });

                TempData["MensagemSucesso"] = "Aula cancelada com sucesso.";
                return RedirectToAction("Index", "AgendasDisponiveis");
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao cancelar a aula: " + ex.Message;
                return RedirectToAction("Index", "AgendasDisponiveis");
            }
        }
    }
}