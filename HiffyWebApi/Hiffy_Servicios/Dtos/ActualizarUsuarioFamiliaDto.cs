using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class ActualizarUsuarioFamiliaDto
    {    
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }  
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; } 
        public int? IdRolFamilia { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? Documento { get; set; }
    }
}
