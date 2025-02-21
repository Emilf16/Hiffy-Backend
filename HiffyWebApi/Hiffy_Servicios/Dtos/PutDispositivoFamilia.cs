using Hiffy_Entidades.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class PutDispositivoFamilia
    {
         
            public int IdDispositivoFamilia { get; set; }
            public string IdDispositivo { get; set; }
            public string NombreDispositivo { get; set; }
            public int IdAreaFamilia { get; set; }
            public EstadoDispositivo Estado { get; set; }
         
        }
    }
