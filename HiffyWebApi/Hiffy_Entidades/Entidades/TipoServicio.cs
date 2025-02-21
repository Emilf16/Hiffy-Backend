using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class TipoServicio
    {
        public int IdTipoServicio { get; set; } // Primary Key
        public string Nombre { get; set; } // Nombre del tipo de servicio
        public string? Descripcion { get; set; } // Descripción opcional
    }
}
