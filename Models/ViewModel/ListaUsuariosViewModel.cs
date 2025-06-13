using System.Collections.Generic;
using music_shed.Models;

namespace music_shed.Models.ViewModel
{
    public class ListaUsuariosViewModel
    {
        public IEnumerable<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public int PaginaAtual  { get; set; }
        public int TotalPaginas { get; set; }
    }
}