using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class PostFamilia
    { 
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Altitud { get; set; }
        public string Longitud { get; set; }
        public int RolFamilia { get; set; } // Este es el rol que viene del frontend
        public List<AreaDelHogarDto> AreasHogar { get; set; } // Este es el listado de areas que viene del frontend 
    }
}
