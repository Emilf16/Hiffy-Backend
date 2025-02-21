using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    
    public class PostAsignacionDto
    {
        public int Id { get; set; } // Identificador único
        public string DeviceCode { get; set; } // Código único del dispositivo
        public int AreaId { get; set; } // AreaId que corresponde a `areaId`
        public int TareaId { get; set; } // TareaId que corresponde a `tareaId`
        public int MiembroId { get; set; } // MiembroId que corresponde a `miembroId`
        public string Descripcion { get; set; } // Descripción de la tarea
        public DateTime FechaHora { get; set; } // Fecha y hora de la asignación, en formato DateTime
        public string Prioridad { get; set; } // Prioridad de la tarea
        public string Estado { get; set; } // Estado de la tarea
        public bool EsRecurrente { get; set; } // Estado de la tarea
    }

    public class FamilyInfoResponse
    {
        public List<ItemDto> FamilyMembers { get; set; }
        public List<ItemDto> HouseAreas { get; set; }
        public List<ItemDto> Tasks { get; set; }
    }

    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
