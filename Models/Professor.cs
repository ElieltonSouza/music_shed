using System;

namespace music_shed.Models
{
    /// <summary>
    /// Representa um professor cadastrado no sistema Music Shed.
    /// </summary>
    public class Professor
    {
        /// <summary>
        /// Identificador único do professor.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do professor.
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// E-mail institucional do professor.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Data de cadastro do professor.
        /// </summary>
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}