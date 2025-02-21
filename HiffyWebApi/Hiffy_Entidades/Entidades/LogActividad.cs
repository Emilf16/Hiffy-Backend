using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class LogActividad
    {
        public int IdLogActividad { get; set; }
        public int IdUsuario { get; set; }
        public string Metodo { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
        public DateTime Fecha { get; set; }
    }
}
