using Microsoft.AspNetCore.Mvc;
using music_shed.Models.ViewModel;
using music_shed.Repositorios;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace music_shed.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepositorio _repo;
        private const int PageSize = 10;

        public UsuariosController(IUsuarioRepositorio repo) => _repo = repo;

        /* -------- LISTA -------- */
        public async Task<IActionResult> Lista(int page = 1)
        {
            if (HttpContext.Session.GetString("Perfil") != "Administrador")
                return Forbid();

            int total = await _repo.ContarAsync();
            int skip  = Math.Max(page - 1, 0) * PageSize;

            var vm = new ListaUsuariosViewModel
            {
                Usuarios     = await _repo.ListarPaginadoAsync(skip, PageSize),
                PaginaAtual  = page,
                TotalPaginas = (int)Math.Ceiling(total / (double)PageSize)
            };
            return View(vm);
        }

        public IActionResult Novo() => RedirectToAction("Cadastrar", "Professores");

        /* -------- EDITAR (GET) -------- */
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            string perfil = HttpContext.Session.GetString("Perfil");
            int?   meuId  = HttpContext.Session.GetInt32("UsuarioId");

            if (id == null) id = meuId;                          // assume o próprio
            if (perfil != "Administrador" && id != meuId) return Forbid();
            if (id == null) return Forbid();

            var usuario = await _repo.ObterPorIdAsync(id.Value);
            if (usuario == null) return NotFound();

            return View(new EditarUsuarioViewModel
            {
                Id   = usuario.Id,
                Nome = usuario.NomeHash
            });
        }

        /* -------- EDITAR (POST) -------- */
        [HttpPost]
        public async Task<IActionResult> Editar(EditarUsuarioViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string perfil = HttpContext.Session.GetString("Perfil");
            int?   meuId  = HttpContext.Session.GetInt32("UsuarioId");

            if (perfil != "Administrador" && vm.Id != meuId) return Forbid();

            await _repo.AtualizarNomeAsync(vm.Id, vm.Nome);
            await _repo.MarcarNomeEditadoManualmenteAsync(vm.Id); // ← ESSA LINHA IMPORTANTE

            if (vm.Id == meuId)
                HttpContext.Session.SetString("NomeUsuario", vm.Nome);

            TempData["MensagemSucesso"] = "Dados atualizados com sucesso!";

            return vm.Id != meuId
                ? RedirectToAction("Lista")
                : RedirectToAction("Index", "Home");
        }
        /* -------- EXCLUIR (GET de confirmação) -------- */
        [HttpGet]
        public async Task<IActionResult> Excluir(int id)
        {
            // apenas administrador
            if (HttpContext.Session.GetString("Perfil") != "Administrador")
                return Forbid();

            var usuario = await _repo.ObterPorIdAsync(id);
            if (usuario == null) return NotFound();

            // força o Razor a usar Views/Usuarios/Excluir.cshtml
            return View("Excluir", usuario);
        }

        /* -------- EXCLUIR (POST – confirmação) -------- */
        [HttpPost, ActionName("Excluir")]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            if (HttpContext.Session.GetString("Perfil") != "Administrador")
                return Forbid();

            await _repo.ExcluirAsync(id);

            TempData["MensagemSucesso"] = "Usuário excluído com sucesso!";
            return RedirectToAction("Lista");
        }
    }
}