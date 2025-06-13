using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace music_shed.Models
{
    /// <summary>
    /// Representa um horário disponível criado por um professor para agendamento de aula.
    /// </summary>
    public class AgendaDisponivel
    {
        /// <summary>
        /// Identificador único da disponibilidade.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do professor responsável por essa agenda.
        /// </summary>
        public int ProfessorId { get; set; }

        /// <summary>
        /// Data e hora de início da aula.
        /// </summary>
        public DateTime DataHora { get; set; }

        /// <summary>
        /// Data e hora de término da aula.
        /// </summary>
        public DateTime HoraFim { get; set; }

        /// <summary>
        /// Indica se a data é uma exceção (feriado ou final de semana).
        /// </summary>
        public bool LiberacaoExcepcional { get; set; } = false;

        /// <summary>
        /// Justificativa para liberar a data excepcional, se aplicável.
        /// </summary>
        public string? Justificativa { get; set; }

        /// <summary>
        /// Tipo de aula: "Particular" ou "Conjunto".
        /// </summary>
        public string TipoAula { get; set; } = "Particular";

        /// <summary>
        /// Lista de IDs dos alunos participantes.
        /// </summary>
        public string AlunosIds { get; set; } = ""; // Ex: "3" ou "2,4,7"
        public string StatusAula { get; set; } = "Agendada"; // Padrão ao criar
        public string? StatusPresenca { get; set; }
        public int? MinutosConfirmacaoPermitida { get; set; }
        public string JustificativaPresencaAntecipada { get; set; }

        [NotMapped]
        public string NomeProfessor { get; set; }
        
        [NotMapped]
        public string NomesAlunos { get; set; }

        public int? MinutosAntecedenciaReagendamentoAluno { get; set; }
        public int? MinutosAntecedenciaCancelamentoAluno { get; set; }
    }
}