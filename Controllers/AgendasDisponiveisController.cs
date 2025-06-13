using Microsoft.AspNetCore.Mvc;
using music_shed.Models;
using music_shed.Models.ViewModel;
using music_shed.Repositorios;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using music_shed.Infraestrutura;
using Dapper;
using System.Data;

namespace music_shed.Controllers
{
    public class AgendasDisponiveisController : Controller
    {
        private readonly IAgendaRepositorio _agendaRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IAlunoPorProfessorRepositorio _associacaoRepositorio;
        private readonly ISolicitacaoReagendamentoRepositorio _reagendamentoRepositorio;
        private readonly IConnectionFactory _connectionFactory;


        public AgendasDisponiveisController(
            IAgendaRepositorio agendaRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IAlunoPorProfessorRepositorio associacaoRepositorio,
            ISolicitacaoReagendamentoRepositorio reagendamentoRepositorio,
            IConnectionFactory connectionFactory) // ⬅️ ADICIONAR AQUI
        {
            _agendaRepositorio = agendaRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _associacaoRepositorio = associacaoRepositorio;
            _reagendamentoRepositorio = reagendamentoRepositorio;
            _connectionFactory = connectionFactory; // ⬅️ INICIALIZAR AQUI
        }

        public async Task<IActionResult> Index()
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (string.IsNullOrEmpty(perfil) || usuarioId == null)
                return Forbid();

            IEnumerable<AgendaDisponivel> agendas;

            if (perfil == "Administrador")
            {
                agendas = await _agendaRepositorio.ListarTodasAsync();
            }
            else if (perfil == "Professor")
            {
                agendas = await _agendaRepositorio.ListarPorProfessorAsync(usuarioId.Value);
            }
            else
            {
                return Forbid();
            }

            return View(agendas);
        }

        [HttpGet]
        public async Task<IActionResult> Criar()
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if ((perfil != "Professor" && perfil != "Administrador") || usuarioId == null)
                return Forbid();

            List<Usuario> alunos;

            if (perfil == "Administrador")
            {
                alunos = (await _usuarioRepositorio.ListarPorPerfilAsync("Aluno")).ToList();
            }
            else
            {
                var idsAssociados = await _associacaoRepositorio.ObterAlunosPorProfessorAsync(usuarioId.Value);
                var todosAlunos = await _usuarioRepositorio.ListarPorPerfilAsync("Aluno");
                alunos = todosAlunos.Where(a => idsAssociados.Contains(a.Id)).ToList();
            }

            using IDbConnection connection = _connectionFactory.CriarConexao();
            var config = await connection.QueryFirstOrDefaultAsync<ConfiguracaoGlobal>(
                "SELECT TOP 1 * FROM ConfiguracoesGlobais ORDER BY Id DESC");

            var vm = new NovaAgendaViewModel
            {
                ListaAlunos = alunos,
                MinutosConfirmacaoPermitida = config?.TempoConfirmacaoPresencaMinutos ?? 1440,
                MinutosAntecedenciaCancelamentoAluno = config?.MinutosAntecedenciaCancelamentoAluno ?? 60,
                MinutosAntecedenciaReagendamentoAluno = config?.MinutosAntecedenciaReagendamentoAluno ?? 60
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Criar(NovaAgendaViewModel vm)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if ((perfil != "Professor" && perfil != "Administrador") || usuarioId == null)
                return Forbid();

            List<Usuario> alunosDisponiveis;
            if (perfil == "Professor")
            {
                var idsAssociados = await _associacaoRepositorio.ObterAlunosPorProfessorAsync(usuarioId.Value);
                var todosAlunos = await _usuarioRepositorio.ListarPorPerfilAsync("Aluno");
                alunosDisponiveis = todosAlunos.Where(a => idsAssociados.Contains(a.Id)).ToList();
            }
            else
            {
                alunosDisponiveis = (await _usuarioRepositorio.ListarPorPerfilAsync("Aluno")).ToList();
            }

            vm.ListaAlunos = alunosDisponiveis;

            if (!ModelState.IsValid)
                return View(vm);

            var alunosNaoPermitidos = vm.AlunosSelecionados.Except(alunosDisponiveis.Select(a => a.Id));
            if (alunosNaoPermitidos.Any())
            {
                ModelState.AddModelError(string.Empty, "Você tentou selecionar alunos que não estão associados a você.");
                return View(vm);
            }

            DateTime dataHora = vm.DataHora!.Value;
            DateTime horaFim = vm.HoraFim!.Value;

            if (horaFim <= dataHora)
            {
                ModelState.AddModelError("HoraFim", "A hora de término deve ser posterior à hora de início.");
                return View(vm);
            }

            bool conflitoProfessor = await _agendaRepositorio.ExisteAgendaNoMesmoHorarioAsync(usuarioId.Value, dataHora, horaFim);
            if (conflitoProfessor)
            {
                ModelState.AddModelError(string.Empty, "Você já possui uma aula nesse intervalo de horário. Edite a agenda existente ou escolha outro horário.");
                return View(vm);
            }

            foreach (var alunoId in vm.AlunosSelecionados)
            {
                (bool conflito, string nomeProfessor) = await _agendaRepositorio.AlunoPossuiConflitoAsync(alunoId, dataHora, horaFim);
                if (conflito)
                {
                    ModelState.AddModelError(string.Empty, $"O aluno com ID {alunoId} já possui uma aula com o professor {nomeProfessor} nesse mesmo intervalo.");
                    return View(vm);
                }
            }

            bool ehFds = dataHora.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            bool ehFeriado = dataHora.Date == new DateTime(dataHora.Year, 9, 7);

            if ((ehFds || ehFeriado) && string.IsNullOrWhiteSpace(vm.Justificativa))
            {
                ModelState.AddModelError("Justificativa", "Justificativa obrigatória para datas excepcionais (feriado ou fim de semana).");
                return View(vm);
            }

            var agenda = new AgendaDisponivel
            {
                ProfessorId = usuarioId.Value,
                DataHora = dataHora,
                HoraFim = horaFim,
                TipoAula = vm.TipoAula,
                AlunosIds = string.Join(",", vm.AlunosSelecionados),
                Justificativa = vm.Justificativa,
                LiberacaoExcepcional = ehFds || ehFeriado,

                MinutosConfirmacaoPermitida = vm.MinutosConfirmacaoPermitida,
                MinutosAntecedenciaCancelamentoAluno = vm.MinutosAntecedenciaCancelamentoAluno,
                MinutosAntecedenciaReagendamentoAluno = vm.MinutosAntecedenciaReagendamentoAluno
            };

            await _agendaRepositorio.InserirAsync(agenda);

            TempData["MensagemSucesso"] = "Agenda criada com sucesso!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null || string.IsNullOrEmpty(perfil))
                return Forbid();

            var agenda = await _agendaRepositorio.ObterPorIdAsync(id);
            if (agenda == null) return NotFound();

            if (agenda.StatusPresenca == "Presente")
            {
                TempData["MensagemErro"] = "Esta agenda já teve presença confirmada e não pode ser editada.";
                return RedirectToAction("Index");
            }

            if (perfil == "Professor" && agenda.ProfessorId != usuarioId.Value)
                return Forbid();

            List<Usuario> listaAlunos;

            if (perfil == "Administrador")
            {
                listaAlunos = (await _usuarioRepositorio.ListarPorPerfilAsync("Aluno")).ToList();
            }
            else
            {
                var idsAlunos = await _associacaoRepositorio.ObterAlunosPorProfessorAsync(usuarioId.Value);
                var todosAlunos = await _usuarioRepositorio.ListarPorPerfilAsync("Aluno");
                listaAlunos = todosAlunos.Where(a => idsAlunos.Contains(a.Id)).ToList();
            }

            var vm = new NovaAgendaViewModel
            {
                Id = agenda.Id,
                DataHora = agenda.DataHora,
                HoraFim = agenda.HoraFim,
                TipoAula = agenda.TipoAula,
                Justificativa = agenda.Justificativa,
                AlunosSelecionados = string.IsNullOrWhiteSpace(agenda.AlunosIds)
                    ? new List<int>()
                    : agenda.AlunosIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(idStr => int.TryParse(idStr, out var idAluno) ? idAluno : -1)
                        .Where(id => id != -1)
                        .ToList(),
                ListaAlunos = listaAlunos,
                MinutosConfirmacaoPermitida = agenda.MinutosConfirmacaoPermitida,
                MinutosAntecedenciaCancelamentoAluno = agenda.MinutosAntecedenciaCancelamentoAluno,
                MinutosAntecedenciaReagendamentoAluno = agenda.MinutosAntecedenciaReagendamentoAluno
            };

            ViewBag.AgendaId = agenda.Id;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(int id, NovaAgendaViewModel vm)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if ((perfil != "Professor" && perfil != "Administrador") || usuarioId == null)
                return Forbid();

            List<Usuario> alunosDisponiveis;
            if (perfil == "Administrador")
            {
                alunosDisponiveis = (await _usuarioRepositorio.ListarPorPerfilAsync("Aluno")).ToList();
            }
            else
            {
                var idsAlunos = await _associacaoRepositorio.ObterAlunosPorProfessorAsync(usuarioId.Value);
                var todosAlunos = await _usuarioRepositorio.ListarPorPerfilAsync("Aluno");
                alunosDisponiveis = todosAlunos.Where(a => idsAlunos.Contains(a.Id)).ToList();
            }

            vm.ListaAlunos = alunosDisponiveis;

            if (!ModelState.IsValid)
                return View(vm);

            var agendaExistente = await _agendaRepositorio.ObterPorIdAsync(id);
            if (agendaExistente == null) return NotFound();

            if (agendaExistente.StatusPresenca == "Presente")
            {
                TempData["MensagemErro"] = "Não é possível editar uma agenda que já teve presença confirmada.";
                return RedirectToAction("Index");
            }

            if (perfil == "Professor" && agendaExistente.ProfessorId != usuarioId.Value)
                return Forbid();

            DateTime dataHora = vm.DataHora!.Value;
            DateTime horaFim = vm.HoraFim!.Value;

            if (horaFim <= dataHora)
            {
                ModelState.AddModelError("HoraFim", "A hora de término deve ser posterior à hora de início.");
                return View(vm);
            }

            bool conflitoProfessor = await _agendaRepositorio.ExisteAgendaNoMesmoHorarioAsync(usuarioId.Value, dataHora, horaFim, id);
            if (conflitoProfessor)
            {
                ModelState.AddModelError(string.Empty, "Você já possui uma agenda que conflita com esse horário.");
                return View(vm);
            }

            foreach (var alunoId in vm.AlunosSelecionados)
            {
                var (conflito, nomeProfessor) = await _agendaRepositorio.AlunoPossuiConflitoAsync(alunoId, dataHora, horaFim, id);
                if (conflito)
                {
                    ModelState.AddModelError(string.Empty, $"O aluno com ID {alunoId} já possui uma aula com o professor {nomeProfessor} nesse mesmo intervalo.");
                    return View(vm);
                }
            }

            agendaExistente.DataHora = dataHora;
            agendaExistente.HoraFim = horaFim;
            agendaExistente.TipoAula = vm.TipoAula;
            agendaExistente.AlunosIds = string.Join(",", vm.AlunosSelecionados);
            agendaExistente.Justificativa = vm.Justificativa;
            agendaExistente.LiberacaoExcepcional = dataHora.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ||
                                                dataHora.Date == new DateTime(dataHora.Year, 9, 7);
            agendaExistente.MinutosConfirmacaoPermitida = vm.MinutosConfirmacaoPermitida;
            agendaExistente.MinutosAntecedenciaCancelamentoAluno = vm.MinutosAntecedenciaCancelamentoAluno;
            agendaExistente.MinutosAntecedenciaReagendamentoAluno = vm.MinutosAntecedenciaReagendamentoAluno;

            await _agendaRepositorio.AtualizarAsync(agendaExistente);

            TempData["MensagemSucesso"] = "Agenda atualizada com sucesso!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Excluir(int id)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null || string.IsNullOrEmpty(perfil))
                return Forbid();

            var agenda = await _agendaRepositorio.ObterPorIdAsync(id);
            if (agenda == null) return NotFound();

            if (perfil == "Professor" && agenda.ProfessorId != usuarioId.Value)
                return Forbid();

            return View("Excluir", agenda);
        }

        [HttpPost, ActionName("Excluir")]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            var agenda = await _agendaRepositorio.ObterPorIdAsync(id);
            if (agenda == null) return NotFound();

            if ((perfil == "Professor" && agenda.ProfessorId != usuarioId) || usuarioId == null)
                return Forbid();

            await _agendaRepositorio.ExcluirAsync(id);

            TempData["MensagemSucesso"] = "Agenda excluída com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> MinhasAulas()
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var alunoId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Aluno" || alunoId == null)
                return Forbid();

            var agendas = await _agendaRepositorio.ListarAgendasDoAlunoAsync(alunoId.Value);
            var viewModels = new List<MinhasAulasViewModel>();

            foreach (var agenda in agendas)
            {
                // Regras de permissão
                bool aulaFutura = agenda.DataHora > DateTime.Now;
                bool naoConfirmada = string.IsNullOrEmpty(agenda.StatusPresenca);
                bool naoCancelada = agenda.StatusAula != "Cancelada";

                viewModels.Add(new MinhasAulasViewModel
                {
                    Id = agenda.Id,
                    DataHoraInicio = agenda.DataHora,
                    DataHoraFim = agenda.HoraFim,
                    TipoAula = agenda.TipoAula,
                    NomeProfessor = agenda.NomeProfessor ?? "Professor",
                    StatusPresenca = agenda.StatusPresenca,
                    StatusAula = agenda.StatusAula,
                    Justificativa = agenda.Justificativa,
                    JustificativaPresencaAntecipada = agenda.JustificativaPresencaAntecipada,

                    PodeCancelar = aulaFutura && naoConfirmada && naoCancelada,
                    PodeSolicitarReagendamento = aulaFutura && naoConfirmada && naoCancelada
                });
            }

            return View("MinhasAulas", viewModels);
        }
        [HttpPost]
        public async Task<IActionResult> Responder(int id, string acao, string? observacaoResposta)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Professor" || usuarioId == null)
                return Forbid();

            var solicitacao = await _reagendamentoRepositorio.ObterPorIdAsync(id);
            if (solicitacao == null)
                return NotFound();

            var agenda = await _agendaRepositorio.ObterPorIdAsync(solicitacao.AgendaId);
            if (agenda == null || agenda.ProfessorId != usuarioId.Value)
                return Forbid();

            string novoStatus = acao == "Aprovar" ? "Aprovado" : "Rejeitado";

            await _reagendamentoRepositorio.AtualizarStatusAsync(id, novoStatus, observacaoResposta);

            TempData["MensagemSucesso"] = $"Solicitação {novoStatus.ToLower()} com sucesso!";
            return RedirectToAction("Pendentes");
        }

        [HttpGet]
        public async Task<IActionResult> Pendentes()
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var professorId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Professor" || professorId == null)
                return Forbid();

            var solicitacoes = await _reagendamentoRepositorio.ListarPendentesDoProfessorAsync(professorId.Value);
            return View("Pendentes", solicitacoes);
        }
    }
}