using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class ActualizarUsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }

        public string Correo { get; set; }
        //public string Contraseña { get; set; }
        //public DateTime FechaRegistro { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public int IdEstadoFamilia { get; set; }
        public int IdEstadoVendedor { get; set; }
        public int IdRol { get; set; }
        public int? IdRolFamilia { get; set; }
        public int? IdFamilia { get; set; }
        public string Descripcion { get; set; }
        public decimal? Valoracion { get; set; }
        public string? CodigoVerificacion { get; set; }
        public DateTime? FechaLimiteCodigo { get; set; }
        public string? FotoUrl { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string Documento { get; set; }
    }
}
