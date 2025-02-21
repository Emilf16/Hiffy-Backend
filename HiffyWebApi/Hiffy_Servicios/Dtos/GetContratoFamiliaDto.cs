using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class GetContratoFamiliaDto
    {

        public string FotoOfertador { get; set; }
        public string CorreoOfertador { get; set; }
        public string NombreOfertador { get; set; }
        public ContratoPersonal ContratoPersonal {  get; set; } 
        public Servicio Servicio { get; set; }

    }
}
