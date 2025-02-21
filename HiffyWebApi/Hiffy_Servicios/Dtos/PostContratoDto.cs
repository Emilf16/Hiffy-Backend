using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class PostContratoDto
    { 
        public int IdServicioContratado { get; set; } 
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; } 
    }
}
