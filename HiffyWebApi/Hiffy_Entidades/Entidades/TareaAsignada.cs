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
    public class TareaAsignada
    {
                
        public int IdTareaAsignada { get; set; }
        public int IdUsuario { get; set; }
        public int IdTareaDomestica { get; set; }
        public int IdAreaFamilia { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; } 
        public string Prioridad { get; set; }
        public bool EsRecurrente { get; set; }
        public DiaSemana? DiaSemana { get; set; }
        public EstadoTarea Estado { get; set; }
        // Relaciones
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }

        [ForeignKey("IdTareaDomestica")]
        public TareaDomestica TareaDomestica { get; set; }

        [ForeignKey("IdAreaFamilia")]
        public AreaDelHogar_Familia AreaFamilia { get; set; }
        public ICollection<RecurrenciaTareas> Recurrencias { get; set; }

        public enum EstadoTarea
        {
            Pendiente = 1,
            EnCurso = 2,
            Completada = 3,
        }

    }

    public class TareaAsignadaDashBoard
    {

        public int IdTareaAsignada { get; set; }
        public int IdUsuario { get; set; }
        public int IdTareaDomestica { get; set; }
        public int IdAreaFamilia { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public string Prioridad { get; set; }
        public bool EsRecurrente { get; set; }
        public DiaSemana? DiaSemana { get; set; }
        public EstadoTarea Estado { get; set; }
        public List<RecurrenciaTareas>? RecurrenciaTareas { get; set; }
        // Relaciones
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }

        [ForeignKey("IdTareaDomestica")]
        public TareaDomestica TareaDomestica { get; set; }

        [ForeignKey("IdAreaFamilia")]
        public AreaDelHogar_Familia AreaFamilia { get; set; }

        

    }
}
