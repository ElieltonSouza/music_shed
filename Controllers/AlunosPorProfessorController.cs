using Microsoft.AspNetCore.Mvc;
using music_shed.Models.ViewModel;
using music_shed.Repositorios;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace music_shed.Controllers
{
    public class AlunosPorProfessorController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IAlunoPorProfessorRepositorio _associacaoRepositorio;

        public AlunosPorProfessorController(
            IUsuarioRepositorio usuarioRepositorio,
            IAlunoPorProfessorRepositorio associacaoRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _associacaoRepositorio = associacaoRepositorio;
        }

        [HttpGet]
        public async Task<IActionResult> Associar()
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var professorId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Professor" || professorId == null)
                return Forbid();

            var todosAlunos = await _usuarioRepositorio.ListarPorPerfilAsync("Aluno");
            var associados = await _associacaoRepositorio.ObterAlunosPorProfessorAsync(professorId.Value);

            var vm = new AssociarAlunosViewModel
            {
                ProfessorId = professorId.Value,
                AlunosDisponiveis = todosAlunos.ToList(),
                AlunosSelecionados = associados
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Associar(AssociarAlunosViewModel vm)
        {
            var perfil = HttpContext.Session.GetString("Perfil");
            var professorId = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Professor" || professorId == null)
                return Forbid();

            await _associacaoRepositorio.RemoverTodosAsync(professorId.Value);
            await _associacaoRepositorio.InserirAsync(professorId.Value, vm.AlunosSelecionados);

            TempData["MensagemSucesso"] = "Associação de alunos atualizada com sucesso!";
            return RedirectToAction("Associar");
        }
    }
}