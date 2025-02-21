using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class ContratoPersonal
    {
        public int IdContrato { get; set; }
        public int IdFamilia { get; set; } 
        public int IdServicioContratado { get; set; }
        public int CodigoVerificacion { get; set; }
        public int CodigoFinalizacion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoContrato Estado { get; set; }
        public int? Valoracion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? MotivoCancelacion { get; set; }


        // Relaciones de claves foráneas
        [ForeignKey("IdFamilia")]
        public Familia Familia { get; set; }

        [ForeignKey("IdServicioContratado")]
        public Servicio Servicio { get; set; }
    }
    public enum EstadoContrato
    {
        Solicitado = 1,   // El contrato ha sido solicitado, pero no ha sido aceptado aún.
        Aceptado = 2,     // El contrato ha sido aceptado.
        EnCurso = 3,      // El contrato está en curso o en ejecución.
        Finalizado = 4,   // El contrato ha sido finalizado.
        Cancelado = 5     // El contrato ha sido cancelado.
    }
}
