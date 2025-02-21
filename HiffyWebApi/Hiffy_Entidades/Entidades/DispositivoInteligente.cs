using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class DispositivoInteligente
    {
        public int IdDispositivo { get; set; }
        public string Estado { get; set; }
        public string Ubicacion { get; set; }
        public float? Altitud { get; set; }
        public float? Longitud { get; set; }
        public DateTime FechaAgregado { get; set; } = DateTime.Now;
    }
}
