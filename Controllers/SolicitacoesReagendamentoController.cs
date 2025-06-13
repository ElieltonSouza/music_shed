using Microsoft.AspNetCore.Mvc;
using music_shed.Models;
using music_shed.Repositorios;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace music_shed.Controllers
{
    public class SolicitacoesReagendamentoController : Controller
    {
        private readonly ISolicitacaoReagendamentoRepositorio _repo;
        private readonly IAgendaRepositorio _agendaRepo;
        private readonly IConfiguracaoGlobalRepositorio _configRepo;

        public SolicitacoesReagendamentoController(
            ISolicitacaoReagendamentoRepositorio repo,
            IAgendaRepositorio agendaRepo,
            IConfiguracaoGlobalRepositorio configRepo)
        {
            _repo = repo;
            _agendaRepo = agendaRepo;
            _configRepo = configRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Nova(int agendaId)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var alunoId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Aluno" || alunoId == null)
                return Forbid();

            var agenda = await _agendaRepo.ObterPorIdAsync(agendaId);
            if (agenda == null || !agenda.AlunosIds.Split(',').Contains(alunoId.Value.ToString()))
                return Forbid();

            int antecedencia = agenda.MinutosAntecedenciaReagendamentoAluno ?? 60;
            if (DateTime.Now > agenda.DataHora.AddMinutes(-antecedencia))
            {
                TempData["MensagemErro"] = $"Solicitações de reagendamento só podem ser feitas com no mínimo {antecedencia} minutos de antecedência.";
                return RedirectToAction("MinhasAulas", "AgendasDisponiveis");
            }

            var solicitacao = new SolicitacaoReagendamento
            {
                AgendaId = agendaId,
                AlunoId = alunoId.Value
            };

            return View(solicitacao);
        }

        [HttpPost]
        public async Task<IActionResult> Nova(SolicitacaoReagendamento model)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var alunoId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Aluno" || alunoId == null)
                return Forbid();

            var jaSolicitou = await _repo.ExisteSolicitacaoValidaAsync(model.AgendaId, alunoId.Value);
            if (jaSolicitou)
            {
                TempData["MensagemErro"] = "Você já possui uma solicitação de reagendamento para esta aula.";
                return RedirectToAction("MinhasAulas", "AgendasDisponiveis");
            }

            if (string.IsNullOrWhiteSpace(model.Justificativa))
            {
                ModelState.AddModelError("Justificativa", "A justificativa é obrigatória.");
                return View(model);
            }

            model.AlunoId = alunoId.Value;
            model.DataSolicitacao = DateTime.Now;
            model.Status = "Pendente";
            model.ObservacaoResposta = null;

            await _repo.InserirAsync(model);

            TempData["MensagemSucesso"] = "Solicitação de reagendamento enviada com sucesso!";
            return RedirectToAction("MinhasAulas", "AgendasDisponiveis");
        }
    }
}