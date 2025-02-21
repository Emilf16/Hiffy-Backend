using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Sexo { get; set; }
        public string Descripcion { get; set; }
        public decimal? Valoracion { get; set; }
        public string? FotoUrl { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int IdRol { get; set; }
        public int? IdFamilia { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NombreFamilia { get; set; }
        public string Documento { get; set; }
        public string? RolNombre { get; set; }  
        public string? RolFamiliaNombre { get; set; }
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public RolFamilia RolFamilia { get; set; } 
        public Rol Rol { get; set; }  
        public EstadoFamilia EstadoFamilia { get; set; }
        public EstadoVendedor EstadoVendedor { get; set; }
    }
}
