using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class TareaDomesticaPostDto
    {
        public int IdTareaDomestica { get; set; }
        public int? IdFamilia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdTipoTarea { get; set; }
        public EstadoTareaDomestica IdEstadoTarea { get; set; }
        public bool Predeterminado { get; set; }
    }

    public class TareaDomesticaGetDto
    {
        public int IdTareaDomestica { get; set; }
        public int? IdFamilia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdTipoTarea { get; set; }
        public EstadoTareaDomestica IdEstadoTarea { get; set; }
        public bool Predeterminado { get; set; }
        public TipoTarea TipoTarea { get; set; }
        
    }
}
