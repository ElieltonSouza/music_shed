using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using music_shed.Servicos;
using music_shed.Models;
using music_shed.Repositorios;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace music_shed.Controllers
{
    /// <summary>
    /// Controlador responsável por gerenciar as ações de autenticação com o Google.
    /// </summary>
    public class LoginsController : Controller
    {
        private readonly ServicoDeHash _servicoDeHash;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        /// <summary>
        /// Construtor da controller com injeção dos serviços de hash e repositório.
        /// </summary>
        /// <param name="servicoDeHash">Serviço de anonimização de dados pessoais.</param>
        /// <param name="usuarioRepositorio">Repositório responsável pelo acesso ao banco de dados de usuários.</param>
        public LoginsController(ServicoDeHash servicoDeHash, IUsuarioRepositorio usuarioRepositorio)
        {
            _servicoDeHash = servicoDeHash;
            _usuarioRepositorio = usuarioRepositorio;
        }

        /// <summary>
        /// Inicia o processo de login com o Google, redirecionando para a página de autenticação externa.
        /// </summary>
        /// <returns>Redirecionamento para o provedor externo de login (Google).</returns>
        public IActionResult Entrar()
        {
            var propriedades = new AuthenticationProperties
            {
                RedirectUri = Url.Action("AposLogin", "Logins")
            };

            return Challenge(propriedades, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Ação executada automaticamente após a autenticação do usuário via Google.
        /// Realiza a coleta e anonimização dos dados recebidos, com base na LGPD.
        /// </summary>
        /// <returns>Redireciona o usuário autenticado para a página inicial.</returns>
        public async Task<IActionResult> AposLogin()
        {
            var identidade = User.Identity as ClaimsIdentity;

            if (identidade != null && identidade.IsAuthenticated)
            {
                // Recuperar dados do Google
                string nomeOriginal = identidade.FindFirst(ClaimTypes.Name)?.Value ?? "Desconhecido";
                string emailOriginal = identidade.FindFirst(ClaimTypes.Email)?.Value ?? "Desconhecido";

                // Anonimizar com hash
                string nomeHash = _servicoDeHash.GerarHash(nomeOriginal);
                string emailHash = _servicoDeHash.GerarHash(emailOriginal);

                // Verificar se o usuário já existe no banco
                bool existe = await _usuarioRepositorio.UsuarioJaExisteAsync(emailHash);

                if (!existe)
                {
                    var usuario = new Usuario
                    {
                        NomeHash = nomeHash,
                        EmailHash = emailHash
                    };

                    await _usuarioRepositorio.InserirAsync(usuario);
                }

                // Armazenar o nome real do usuário na sessão
                HttpContext.Session.SetString("NomeUsuario", nomeOriginal);

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Erro");
        }

        /// <summary>
        /// Realiza o logout do usuário, encerrando a sessão de autenticação.
        /// </summary>
        /// <returns>Redireciona o usuário para a página inicial.</returns>
        public async Task<IActionResult> Sair()
        {
            // Finaliza a autenticação e limpa a sessão
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}