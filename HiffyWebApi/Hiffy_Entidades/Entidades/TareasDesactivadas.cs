using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
        public class TareasDesactivadas
        {
            public int IdTareaDesactivada { get; set; }
            public int IdFamilia { get; set; }
        public EstadoTareaDomestica IdEstadoTarea { get; set; }
        public int IdTareaDomestica { get; set; }
            [ForeignKey("IdFamilia")]
            public virtual Familia? Familia { get; set; }
            [ForeignKey("IdTareaDomestica")]
            public virtual TareaDomestica? TareaDomestica { get; set; }

        }
    
}
