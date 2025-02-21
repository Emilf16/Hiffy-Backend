using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class AreaDelHogar_Familia
    {
        public int IdAreaFamilia { get; set; }
        public int IdFamilia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Predeterminado { get; set; }
        public EstadoAreasDelHogar IdEstadoAreasDelHogar {get; set;}
    }

    public enum EstadoAreasDelHogar
    {
        Activo = 1,
        Inactivo = 2,

    }
}
