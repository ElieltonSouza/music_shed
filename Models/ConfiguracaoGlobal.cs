using System;
using System.ComponentModel.DataAnnotations;

namespace music_shed.Models
{
    public class ConfiguracaoGlobal
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tempo para confirmação de presença (minutos)")]
        public int TempoConfirmacaoPresencaMinutos { get; set; }

        [Required]
        [Display(Name = "Antecedência mínima para cancelamento (minutos)")]
        public int MinutosAntecedenciaCancelamentoAluno { get; set; }

        [Required]
        [Display(Name = "Antecedência mínima para reagendamento (minutos)")]
        public int MinutosAntecedenciaReagendamentoAluno { get; set; }

        public DateTime CriadoEm { get; set; }
    }
}