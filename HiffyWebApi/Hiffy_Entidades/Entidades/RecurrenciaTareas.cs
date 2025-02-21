using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hiffy_Entidades.Entidades.TareaAsignada;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hiffy_Entidades.Entidades
{
    public class RecurrenciaTareas
    {
        public int IdRecurrencia { get; set; }  
        public int IdTareaAsignada { get; set; }       
        public EstadoTarea Estado { get; set; }
        public DateTime FechaDia { get; set; }
        [ForeignKey("IdTareaAsignada")]
        public TareaAsignada TareaAsignada { get; set; }

    }

    public enum DiaSemana
    {
        Lunes = 1,
        Martes = 2,
        Miercoles = 3,
        Jueves = 4,
        Viernes = 5,
        Sabado = 6,
        Domingo = 7
    }

}
