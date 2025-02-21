using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class DispositivoFamilia
    {
        public int IdDispositivoFamilia { get; set; }
        public string IdDispositivo { get; set; }
        public string NombreDispositivo { get; set; }
        public int IdAreaFamilia { get; set; }
        public EstadoDispositivo Estado { get; set; }

        [ForeignKey("IdAreaFamilia")]
        public virtual AreaDelHogar_Familia AreaDelHogar_Familia { get; set; }
    }

    public enum EstadoDispositivo
    {
        Solicitado = 1,   // El dispositivo ha sido solicitado, pero no ha sido aceptado aún.
        Aceptado = 2,     // El dispositivo ha sido aceptado.
        Desactivado = 3,      // El dispositivo está desactivado. 
    }
}
