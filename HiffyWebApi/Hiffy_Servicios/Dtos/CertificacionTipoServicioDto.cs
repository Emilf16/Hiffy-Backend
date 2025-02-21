using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class CertificacionTipoServicioDto
    {
        public int IdCertificacion { get; set; }
        public List<int> TipoServicioIds { get; set; }
        public bool Aprobar { get; set; }
        public string? ComentarioCancelado { get; set; }
    }
}
