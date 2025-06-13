namespace music_shed.Models
{
    /// <summary>
    /// Representa a associação entre um professor e um aluno.
    /// </summary>
    public class AlunoPorProfessor
    {
        public int ProfessorId { get; set; }
        public int AlunoId { get; set; }
    }
}