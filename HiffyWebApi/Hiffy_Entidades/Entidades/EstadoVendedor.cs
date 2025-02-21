using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public  class EstadoVendedor
    {
        public int IdEstadoVendedor { get; set; }
        public string Descripcion { get; set; } // Renombrado para evitar conflicto con el nombre de la clase
        public bool Activo { get; set; } 
        public bool Inactivo { get; set; }
        public bool Suspendida { get; set; }
        public bool PendienteValidacion { get; set; }
        public DateTime FechaCreacion { get; set; }
         
    }
}
