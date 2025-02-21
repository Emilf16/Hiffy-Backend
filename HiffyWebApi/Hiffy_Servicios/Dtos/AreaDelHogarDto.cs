using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hiffy_Entidades.Entidades;

namespace Hiffy_Servicios.Dtos
{
    public class AreaDelHogarDto
    {
        public int IdAreaFamilia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Predeterminado { get; set; }
        public EstadoAreasDelHogar IdEstadoAreasDelHogar { get; set; }
    }
}
