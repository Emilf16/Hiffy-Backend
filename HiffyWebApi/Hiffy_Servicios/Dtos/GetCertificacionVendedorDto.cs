using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class GetCertificacionVendedorDto
    {
        public CertificacionVendedor CertificacionVendedor { get; set; }

        public TipoServicio TipoServicio { get; set; }
    }
}
