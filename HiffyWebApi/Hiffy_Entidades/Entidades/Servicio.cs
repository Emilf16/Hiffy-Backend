using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class Servicio
    {
        public int IdServicio { get; set; } // Primary Key
        public int IdUsuario { get; set; } // Foreign Key - Usuario que ofrece el servicio
        public int IdTipoServicio { get; set; } // Foreign Key - Tipo de servicio ofertado
        public string Nombre { get; set; } // Nombre del servicio ofertado
        public string? Descripcion { get; set; } // Descripción opcional del servicio
        public decimal Precio { get; set; } // Precio del servicio
        public string Disponibilidad { get; set; } // Disponibilidad del servicio (ejemplo: horario)
        public DateTime FechaPublicacion { get; set; } // Fecha en que se publicó el servicio

        // Relaciones opcionales (pueden ser incluidas si trabajas con EF Core)
        // public Usuario Usuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }
        [ForeignKey("IdTipoServicio")]
        public TipoServicio TipoServicio { get; set; }
    }
}
