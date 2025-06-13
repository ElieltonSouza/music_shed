using Microsoft.AspNetCore.Mvc;
using music_shed.Models;
using music_shed.Models.ViewModel;
using music_shed.Repositorios;
using music_shed.Servicos;
using System.Threading.Tasks;

namespace music_shed.Controllers
{
    public class ProfessoresController : Controller
    {
        private readonly ServicoDeHash _servicoDeHash;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public ProfessoresController(ServicoDeHash servicoDeHash, IUsuarioRepositorio usuarioRepositorio)
        {
            _servicoDeHash = servicoDeHash;
            _usuarioRepositorio = usuarioRepositorio;
        }

        // GET: /Professores/Cadastrar
        [HttpGet]
        public IActionResult Cadastrar()
        {
            return View();
        }

        // POST: /Professores/Cadastrar
        [HttpPost]
        public async Task<IActionResult> Cadastrar(CadastrarProfessorViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            // Validação extra: senhas devem coincidir
            if (viewModel.Senha != viewModel.ConfirmarSenha)
            {
                ModelState.AddModelError("ConfirmarSenha", "A confirmação da senha não confere.");
                return View(viewModel);
            }

            // Gera hash do e-mail para anonimização (LGPD)
            var emailHash = _servicoDeHash.GerarHash(viewModel.Email);

            // Verifica duplicidade
            bool existe = await _usuarioRepositorio.UsuarioJaExisteAsync(emailHash);
            if (existe)
            {
                ModelState.AddModelError("Email", "Já existe um usuário com este e-mail.");
                return View(viewModel);
            }

            // Hash da senha
            var senhaHash = _servicoDeHash.GerarHash(viewModel.Senha);

            // Monta objeto de domínio
            var novoProfessor = new Usuario
            {
                NomeHash  = viewModel.Nome,   // Nome visível, sem hash
                EmailHash = emailHash,
                SenhaHash = senhaHash,
                Perfil    = "Professor"
            };

            // Insere no banco incluindo SenhaHash
            await _usuarioRepositorio.CriarAsync(novoProfessor, incluiSenha: true);

            TempData["MensagemSucesso"] = "Professor cadastrado com sucesso!";
            return RedirectToAction(nameof(Cadastrar));
        }
    }
}