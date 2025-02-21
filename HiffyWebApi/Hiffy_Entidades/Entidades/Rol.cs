using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Entidades.Entidades
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsVendedor { get; set; }
        public bool EsAdmin { get; set; }
        public bool EsUsuarioFamilia { get; set; }
        public bool EsAmbos { get; set; }
    }
}
