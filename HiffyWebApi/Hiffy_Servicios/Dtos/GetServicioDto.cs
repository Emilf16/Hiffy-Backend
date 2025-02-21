﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class GetServicioDto
    { 
            public int IdServicio { get; set; } // Primary Key
            public int IdUsuario { get; set; } // Foreign Key - Usuario que ofrece el servicio
            public int IdTipoServicio { get; set; } // Foreign Key - Tipo de servicio ofertado
            public string Nombre { get; set; } // Nombre del servicio ofertado
            public string? Descripcion { get; set; } // Descripción opcional del servicio
            public decimal Precio { get; set; } // Precio del servicio
            public string Disponibilidad { get; set; } // Disponibilidad del servicio (ejemplo: horario)
            public DateTime FechaPublicacion { get; set; } // Fecha en que se publicó el servicio
            
            public string VendedorFoto   { get; set; }
            public string VendedorNombre { get; set; }
        public string VendedorCorreo { get; set; }

        public string? Valoracion {  get; set; }
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }

        // Relaciones opcionales (pueden ser incluidas si trabajas con EF Core)
        // public Usuario Usuario { get; set; }
        // public TipoServicio TipoServicio { get; set; }
    }
    }
