using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class CertificacionesStatusDto
    {
        public List<CertificacionVendedorDto> CertificacionesAprobadas {  get; set; }
        public List<CertificacionVendedorDto> CertificacionesRechazadas { get; set; }
        public List<CertificacionVendedorDto> CertificacionesPendientes { get; set; }
    }
}
