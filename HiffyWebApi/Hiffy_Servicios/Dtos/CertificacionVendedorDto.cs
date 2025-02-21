using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class CertificacionVendedorDto
    {
        public int IdCertificacion { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string UrlArchivo { get; set; }
        public DateTime FechaCertificacion { get; set; }
        public string ComentarioCancelado { get; set; }

    }
}
