using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hiffy_Entidades.Entidades.TareaAsignada;

namespace Hiffy_Servicios.Dtos
{
    public class TareaAsignadaDto : IValidatableObject
    {
        [Key]
        public int IdTareaAsignada { get; set; }

        [Required]
        public int IdTareaDomestica { get; set; }

        [Required]
        public int IdAreaFamilia { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "La prioridad no puede superar los 20 caracteres.")]
        public string Prioridad { get; set; }

        [Required]
        public EstadoTarea Estado { get; set; }

        // Parámetros de recurrencia
        [Required]
        public bool EsRecurrente { get; set; }

        public DiaSemana? DiaSemana { get; set; }

        [Required]
        public DateTime HoraInicio { get; set; }

        [Required]
        public DateTime HoraFin { get; set; }

        // Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EsRecurrente && DiaSemana == null)
            {
                yield return new ValidationResult(
                    "El campo DiaSemana es obligatorio si la tarea es recurrente.",
                    new[] { nameof(DiaSemana) }
                );
            }
        }
    }
}
