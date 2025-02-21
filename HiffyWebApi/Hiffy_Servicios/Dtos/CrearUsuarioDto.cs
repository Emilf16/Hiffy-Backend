using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class CrearUsuarioDto
    {

        public string Nombre { get; set; } 
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public string Sexo { get; set; } 
        public int IdRol { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string Documento { get; set; }
        public DateTime FechaNacimiento { get; set; }


    }
}
