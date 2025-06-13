using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace music_shed.Models.ViewModel
{
    /// <summary>
    /// ViewModel usado para criação e edição de uma agenda disponível pelo professor.
    /// </summary>
    public class NovaAgendaViewModel
    {
        /// <summary>
        /// Identificador da agenda (usado para edição).
        /// </summary>
        public int Id { get; set; }

        [Required(ErrorMessage = "A data e hora de início são obrigatórias.")]
        [DataType(DataType.DateTime)]
        public DateTime? DataHora { get; set; }

        /// <summary>
        /// Data e hora de término da aula.
        /// </summary>
        [Required(ErrorMessage = "A data e hora de término são obrigatórias.")]
        [DataType(DataType.DateTime)]
        public DateTime? HoraFim { get; set; }

        [Required(ErrorMessage = "O tipo de aula é obrigatório.")]
        public string TipoAula { get; set; } = "Particular";

        [Display(Name = "Alunos")]
        public List<int> AlunosSelecionados { get; set; } = new();

        [Display(Name = "Justificativa (para exceção)")]
        public string? Justificativa { get; set; }

        /// <summary>
        /// Lista completa de alunos disponíveis para seleção.
        /// </summary>
        public List<Usuario> ListaAlunos { get; set; } = new();
        public int? MinutosConfirmacaoPermitida { get; set; }
        public int? MinutosAntecedenciaCancelamentoAluno { get; set; }
        public int? MinutosAntecedenciaReagendamentoAluno { get; set; }
    }
}