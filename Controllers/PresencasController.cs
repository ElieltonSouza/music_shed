using Microsoft.AspNetCore.Mvc;
using music_shed.Infraestrutura;
using music_shed.Repositorios;
using System.Threading.Tasks;
using System;
using Dapper;

namespace music_shed.Controllers
{
    public class PresencasController : Controller
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IAgendaRepositorio _agendaRepositorio;
        private readonly IConfiguracaoGlobalRepositorio _configuracaoGlobalRepositorio;

        public PresencasController(IConnectionFactory connectionFactory,
                                   IAgendaRepositorio agendaRepositorio,
                                   IConfiguracaoGlobalRepositorio configuracaoGlobalRepositorio)
        {
            _connectionFactory = connectionFactory;
            _agendaRepositorio = agendaRepositorio;
            _configuracaoGlobalRepositorio = configuracaoGlobalRepositorio;
        }

        [HttpPost]
        public async Task<IActionResult> Confirmar(int agendaId)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Professor" && perfil != "Administrador")
                return Forbid();

            var agenda = await _agendaRepositorio.ObterPorIdAsync(agendaId);
            if (agenda == null) return NotFound();

            if (perfil == "Professor" && agenda.ProfessorId != usuarioId)
                return Forbid();

            // ✅ Obtém a configuração global do sistema
            var configuracoes = await _configuracaoGlobalRepositorio.ObterAsync();
            var minutosPermitidos = configuracoes.TempoConfirmacaoPresencaMinutos;

            var agora = DateTime.Now;
            var inicioAula = agenda.DataHora;
            var fimLimite = inicioAula.AddMinutes(minutosPermitidos);

            if (perfil == "Professor")
            {
                if (agora < inicioAula)
                {
                    TempData["MensagemErro"] = "A aula ainda não começou. Justifique o motivo para confirmar a presença adiantada.";
                    return RedirectToAction("JustificarAdiantada", new { agendaId });
                }
                else if (agora > fimLimite)
                {
                    TempData["MensagemErro"] = $"O prazo de {minutosPermitidos} minutos para confirmar a presença expirou. Apenas administradores podem confirmar agora.";
                    return RedirectToAction("Index", "AgendasDisponiveis");
                }
            }

            using var con = _connectionFactory.CriarConexao();
            string sql = @"UPDATE AgendasDisponiveis 
                        SET StatusPresenca = @Status, 
                            DataHoraConfirmacaoPresenca = @DataHora 
                        WHERE Id = @Id";

            await con.ExecuteAsync(sql, new
            {
                Status = "Presente",
                DataHora = DateTime.Now,
                Id = agendaId
            });

            TempData["MensagemSucesso"] = "Presença registrada com sucesso.";
            return RedirectToAction("Index", "AgendasDisponiveis");
        }

        [HttpGet]
        public async Task<IActionResult> JustificarAdiantada(int agendaId)
        {
            var agenda = await _agendaRepositorio.ObterPorIdAsync(agendaId);
            if (agenda == null) return NotFound();

            ViewBag.AgendaId = agenda.Id;
            ViewBag.DataHora = agenda.DataHora.ToString("dd/MM/yyyy HH:mm");

            // Verifica se tem alerta vindo de redirecionamento
            if (TempData.ContainsKey("MensagemErro"))
                ViewBag.MensagemAlerta = TempData["MensagemErro"];

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> JustificarAdiantada(int agendaId, string justificativa)
        {
            if (string.IsNullOrWhiteSpace(justificativa))
            {
                TempData["MensagemErro"] = "Justificativa obrigatória para confirmar presença antes do horário.";
                return RedirectToAction("JustificarAdiantada", new { agendaId });
            }

            using var con = _connectionFactory.CriarConexao();
            string sql = @"UPDATE AgendasDisponiveis 
                           SET StatusPresenca = @Status, 
                               Justificativa = @Justificativa,
                               JustificativaPresencaAntecipada = @Justificativa,
                               DataHoraConfirmacaoPresenca = @DataHora 
                           WHERE Id = @Id";

            await con.ExecuteAsync(sql, new
            {
                Status = "Presente",
                Justificativa = justificativa,
                DataHora = DateTime.Now,
                Id = agendaId
            });

            TempData["MensagemSucesso"] = "Presença confirmada com justificativa.";
            return RedirectToAction("Index", "AgendasDisponiveis");
        }
        [HttpPost]
        public async Task<IActionResult> Cancelar(int agendaId)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Professor" && perfil != "Administrador")
                return Forbid();

            var agenda = await _agendaRepositorio.ObterPorIdAsync(agendaId);
            if (agenda == null) return NotFound();

            if (perfil == "Professor" && agenda.ProfessorId != usuarioId)
                return Forbid();

            using var con = _connectionFactory.CriarConexao();
            string sql = @"UPDATE AgendasDisponiveis 
                        SET StatusPresenca = NULL, 
                            Justificativa = NULL, 
                            JustificativaPresencaAntecipada = NULL, 
                            DataHoraConfirmacaoPresenca = NULL
                        WHERE Id = @Id";

            await con.ExecuteAsync(sql, new { Id = agendaId });

            TempData["MensagemSucesso"] = "Presença cancelada com sucesso.";
            return RedirectToAction("Index", "AgendasDisponiveis");
        }
    }
}