using System.ComponentModel.DataAnnotations;

namespace music_shed.Models.ViewModel
{
    /// <summary>
    /// ViewModel utilizada para cadastrar manualmente professores no sistema.
    /// </summary>
    public class CadastrarProfessorViewModel
    {
        /// <summary>
        /// Nome completo do professor.
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        /// <summary>
        /// E-mail do professor (utilizado para login).
        /// </summary>
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Confirmação de senha obrigatória.")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        [DataType(DataType.Password)]
        public string ConfirmarSenha { get; set; }
    }
}