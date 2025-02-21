using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class LogError
    {
        public int IdLogActividad { get; set; }
        public int IdUsuario { get; set; }
        public string Mensaje { get; set; }
        public string Fuente { get; set; }
        public string StackTrace { get; set; }
        public DateTime Fecha { get; set; }
    }
}
