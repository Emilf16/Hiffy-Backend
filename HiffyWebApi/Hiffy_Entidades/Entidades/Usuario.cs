using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
       
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public int IdEstadoFamilia{ get; set; }
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
        public string? Documento { get; set; }

        public string? Altitud { get; set; }
        public string? Longitud { get; set; }
        // Relaciones de claves foráneas
        [ForeignKey("IdTipoDocumento")]
        public TipoDocumento TipoDocumento { get; set; }

        [ForeignKey("IdEstadoFamilia")]
        public EstadoFamilia EstadoFamilia { get; set; }
        [ForeignKey("IdEstadoVendedor")]
        public EstadoVendedor EstadoVendedor { get; set; }
        [ForeignKey("IdRol")]
        public Rol Rol { get; set; }
        [ForeignKey("IdFamilia")]
        public Familia Familia { get; set; }
        [ForeignKey("IdRolFamilia")]
        public RolFamilia RolFamilia { get; set; }
    }
}
