using System.ComponentModel.DataAnnotations;

namespace music_shed.Models.ViewModel
{
    public class EditarUsuarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }
    }
}