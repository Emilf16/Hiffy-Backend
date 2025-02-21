using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hiffy_Entidades.Entidades.TareaAsignada;

namespace Hiffy_Servicios.Dtos
{
    public class TareaContratoCalendarioDto
    {
        public DateTime Dia { get; set; }
        public int? IdTareaAsignada { get; set; }
        public int? IdTareaDomestica { get; set; }
        public int? IdArea { get; set; }
        public string Area { get; set; }
        public string Tarea { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public int? IdUsuario { get; set; }
        public string Persona { get; set; }
        public string FotoUrl { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoTarea Estado { get; set; }
        public EstadoContrato? EstadoContrato { get; set; }
        public int? IdServicio { get; set; }
        public string Prioridad { get; set; }
        public bool EsContrato { get; set; }
        public bool? EsRecurrente { get; set; }
        public int? ContratoId { get; set; }
        public DiaSemana? DiaSemana { get; set; }
        public DateTime? HoraInicio { get; set; }
        public DateTime? HoraFin { get; set; }
        public List<RecurrenciaTareas>? RecurrenciaTareas { get; set; }
    }

    public class TareasContratosPorDiaDTO
    {
        public DateTime Dia { get; set; }
        public List<TareaContratoCalendarioDto> ListaAsignaciones { get; set; }
    }
}
