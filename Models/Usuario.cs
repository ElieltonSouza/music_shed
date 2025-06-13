using System;

namespace music_shed.Models
{
    /// <summary>
    /// Representa um usuário autenticado via Google ou cadastrado manualmente, com dados conforme LGPD.
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome completo (em texto claro para exibição).
        /// </summary>
        public string NomeHash { get; set; } = null!;

        /// <summary>
        /// E-mail armazenado de forma anonimizada (hash).
        /// </summary>
        public string EmailHash { get; set; } = null!;

        /// <summary>
        /// Data de cadastro no sistema.
        /// </summary>
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        /// <summary>
        /// Perfil de acesso (Aluno ou Professor).
        /// </summary>
        public string Perfil { get; set; } = "Aluno";

        /// <summary>
        /// Hash da senha (usado apenas por professores).
        /// </summary>
        public string? SenhaHash { get; set; }

        /// <summary>
        /// Indica se o nome foi editado manualmente pelo usuário.
        /// Quando true, o nome não será sobrescrito pelo nome da conta Google.
        /// </summary>
        public bool NomeEditadoManualmente { get; set; } = false;

        /// <summary>
        /// Propriedade auxiliar para exibição do nome.
        /// </summary>
        public string Nome => NomeHash;
    }
}