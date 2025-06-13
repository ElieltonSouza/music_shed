using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace music_shed.Models.ViewModel
{
    public class AssociarAlunosViewModel
    {
        public int ProfessorId { get; set; }

        [Display(Name = "Alunos disponíveis")]
        public List<Usuario> AlunosDisponiveis { get; set; } = new();

        [Display(Name = "Alunos selecionados")]
        public List<int> AlunosSelecionados { get; set; } = new();
    }
}