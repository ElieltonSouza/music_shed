using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using music_shed.Models;
using music_shed.Models.ViewModel;
using music_shed.Repositorios;
using music_shed.Servicos;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace music_shed.Controllers
{
    public class LoginsController : Controller
    {
        private readonly ServicoDeHash _servicoDeHash;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public LoginsController(ServicoDeHash servicoDeHash, IUsuarioRepositorio usuarioRepositorio)
        {
            _servicoDeHash      = servicoDeHash;
            _usuarioRepositorio = usuarioRepositorio;
        }

        /* ---------------------------------------------------------
           TELAS DE LOGIN
        --------------------------------------------------------- */
        [HttpGet] public IActionResult EscolherPerfil()  => View();
        [HttpGet] public IActionResult LoginProfessor() => View(new LoginViewModel());
        [HttpGet] public IActionResult LoginAdmin()     => View("LoginAdmin", new LoginViewModel());

        /* ---------------------------------------------------------
           LOGIN   —   ADMINISTRAÇÃO
        --------------------------------------------------------- */
        [HttpPost]
        public async Task<IActionResult> LoginAdmin(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View("LoginAdmin", vm);

            var emailHash = _servicoDeHash.GerarHash(vm.Email.Trim());
            var senhaHash = _servicoDeHash.GerarHash(vm.Senha.Trim());
            var usuario   = await _usuarioRepositorio.ObterPorEmailHashAsync(emailHash);

            bool ok = usuario != null &&
                      usuario.Perfil == "Administrador" &&
                      usuario.SenhaHash == senhaHash;

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Credenciais ou perfil inválidos.");
                return View("LoginAdmin", vm);
            }

            /* ---- Cookie de autenticação ---- */
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  usuario.NomeHash),
                new Claim(ClaimTypes.Email, vm.Email),
                new Claim("Perfil",         "Administrador")
            };
            var ident = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(ident));

            /* ---- Session ---- */
            HttpContext.Session.SetString("Perfil",      "Administrador");
            HttpContext.Session.SetString("NomeUsuario", usuario.NomeHash);
            HttpContext.Session.SetString("EmailUsuario", vm.Email);
            HttpContext.Session.SetInt32 ("UsuarioId",   usuario.Id);

            return RedirectToAction("Lista", "Usuarios");
        }

        /* ---------------------------------------------------------
           LOGIN   —   PROFESSOR
        --------------------------------------------------------- */
        [HttpPost]
        public async Task<IActionResult> LoginProfessor(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var emailHash = _servicoDeHash.GerarHash(vm.Email.Trim());
            var usuario   = await _usuarioRepositorio.ObterPorEmailHashAsync(emailHash);

            if (usuario == null || usuario.Perfil != "Professor")
            {
                ModelState.AddModelError(string.Empty, "Professor não encontrado ou perfil inválido.");
                return View(vm);
            }

            var senhaHashDigitada = _servicoDeHash.GerarHash(vm.Senha.Trim());
            if (usuario.SenhaHash != senhaHashDigitada)
            {
                ModelState.AddModelError(string.Empty, "Senha incorreta.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  usuario.NomeHash),
                new Claim(ClaimTypes.Email, vm.Email),
                new Claim("Perfil",         "Professor")
            };
            var ident = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(ident));

            HttpContext.Session.SetString("NomeUsuario", usuario.NomeHash);
            HttpContext.Session.SetString("Perfil",      "Professor");
            HttpContext.Session.SetInt32 ("UsuarioId",   usuario.Id);

            return RedirectToAction("Index", "Home");
        }

        /* ---------------------------------------------------------
           LOGIN  —  GOOGLE (ALUNOS)
        --------------------------------------------------------- */
        public IActionResult Entrar(string perfil)
        {
            if (string.IsNullOrEmpty(perfil) || (perfil != "Aluno" && perfil != "Professor"))
                return RedirectToAction("EscolherPerfil");

            if (perfil == "Professor")
                return RedirectToAction("LoginProfessor");

            TempData["PerfilEscolhido"] = perfil;
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action("AposLogin", "Logins")
            }, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> AposLogin()
        {
            if (!(User.Identity is ClaimsIdentity id) || !id.IsAuthenticated)
                return RedirectToAction("Erro");

            string nomeOriginal  = id.FindFirst(ClaimTypes.Name )?.Value ?? "Desconhecido";
            string emailOriginal = id.FindFirst(ClaimTypes.Email)?.Value ?? "Desconhecido";
            string emailHash     = _servicoDeHash.GerarHash(emailOriginal.Trim());

            // Tenta obter o usuário existente
            var usuario = await _usuarioRepositorio.ObterPorEmailHashAsync(emailHash);

            if (usuario == null)
            {
                // Se não existe, cria um novo
                var novo = new Usuario
                {
                    NomeHash  = nomeOriginal,
                    EmailHash = emailHash,
                    Perfil    = TempData["PerfilEscolhido"]?.ToString() ?? "Aluno",
                    DataCadastro = DateTime.Now,
                    NomeEditadoManualmente = false
                };
                await _usuarioRepositorio.InserirAsync(novo);
                usuario = novo;
            }
            else
            {
                // Só atualiza o nome se o usuário ainda não tiver editado manualmente
                if (!usuario.NomeEditadoManualmente && usuario.NomeHash != nomeOriginal)
                {
                    await _usuarioRepositorio.AtualizarNomeAsync(usuario.Id, nomeOriginal);
                }
            }

            // Sessões
            HttpContext.Session.SetString("NomeUsuario", usuario.NomeHash);
            HttpContext.Session.SetString("Perfil",      usuario.Perfil);
            HttpContext.Session.SetInt32 ("UsuarioId",   usuario.Id);

            return RedirectToAction("Index", "Home");
        }
        /* ---------------------------------------------------------
           SAIR
        --------------------------------------------------------- */
        public async Task<IActionResult> Sair()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}