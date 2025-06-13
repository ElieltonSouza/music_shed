using System;

namespace music_shed.Models
{
    public class SolicitacaoReagendamento
    {
        public int Id { get; set; }

        public int AgendaId { get; set; }
        public AgendaDisponivel? Agenda { get; set; }

        public int AlunoId { get; set; }
        public Usuario? Aluno { get; set; }

        public string Justificativa { get; set; } = string.Empty;

        public DateTime DataSolicitacao { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pendente";

        public string? ObservacaoResposta { get; set; }

        public string NomeAluno => Aluno?.Nome ?? $"Aluno {AlunoId}";

        public DateTime? DataAula => Agenda?.DataHora;
    }
}