using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class TareaDomestica
    {
        public int IdTareaDomestica { get; set; }
        public int? IdFamilia { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdTipoTarea { get; set; }
        public EstadoTareaDomestica IdEstadoTarea { get; set; }
        public bool Predeterminado { get; set; }
        [ForeignKey("IdTipoTarea")]
        public virtual TipoTarea TipoTarea { get; set; }
        [ForeignKey("IdFamilia")]
        public virtual Familia? Familia { get; set; }
        //[ForeignKey("IdEstadoTarea")]
        //public virtual EstadoTareas EstadoTareas { get; set; }
    }

    public enum EstadoTareaDomestica
    {
        Activo = 1,
        Desactivada = 2,

    }


}
