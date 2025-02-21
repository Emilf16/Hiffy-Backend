using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class PostCertificacionDto
    {
        public DateTime FechaCertificacion { get; set; }
        public string Descripcion { get; set; }
        public IFormFile Archivo { get; set; }
    }
}
