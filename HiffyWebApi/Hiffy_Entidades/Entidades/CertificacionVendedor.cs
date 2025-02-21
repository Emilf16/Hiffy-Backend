using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class CertificacionVendedor
    {
        public int IdCertificacion { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string UrlArchivo { get; set; }
        public DateTime FechaCertificacion { get; set; }

        // Relación con la tabla Usuario
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }
    }
}
