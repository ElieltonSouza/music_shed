using System;

namespace music_shed.Models.ViewModel
{
    public class MinhasAulasViewModel
    {
        public int Id { get; set; }

        public DateTime DataHoraInicio { get; set; }

        public DateTime DataHoraFim { get; set; }

        // Propriedades auxiliares para compatibilidade com a view
        public DateTime DataHora => DataHoraInicio;

        public DateTime HoraFim => DataHoraFim;

        public string TipoAula { get; set; }

        public string NomeProfessor { get; set; }

        public string NomesAlunos { get; set; }

        public string? StatusPresenca { get; set; }

        public string? StatusAula { get; set; }

        public bool PodeCancelar { get; set; }

        public bool PodeSolicitarReagendamento { get; set; }

        public string? Justificativa { get; set; }

        public string? JustificativaPresencaAntecipada { get; set; }

        public bool LiberacaoExcepcional { get; set; }
        public bool JaSolicitouReagendamento { get; set; }
    }
}