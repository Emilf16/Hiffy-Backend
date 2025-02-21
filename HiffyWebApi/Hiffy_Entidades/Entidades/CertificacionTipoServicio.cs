using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class CertificacionTipoServicio
    {
        public int IdCertificacionTipoServicio { get; set; } // Primary Key
        public int IdCertificacion { get; set; } // Foreign Key - Certificación del vendedor
        public int IdTipoServicio { get; set; } // Foreign Key - Tipo de servicio habilitado


        [ForeignKey("IdCertificacion")]
        public CertificacionVendedor CertificacionVendedor { get; set; }

        [ForeignKey("IdTipoServicio")]
        public TipoServicio TipoServicio { get; set; }
    }
}
