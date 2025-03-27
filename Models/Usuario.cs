using System;

namespace music_shed.Models
{
    /// <summary>
    /// Representa um usuário autenticado via Google, com dados anonimizados conforme LGPD.
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do usuário armazenado de forma anonimizada (hash).
        /// </summary>
        public string NomeHash { get; set; }

        /// <summary>
        /// E-mail do usuário armazenado de forma anonimizada (hash).
        /// </summary>
        public string EmailHash { get; set; }

        /// <summary>
        /// Data e hora do cadastro do usuário no sistema.
        /// </summary>
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}