using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{ 
    public class AyudaEnLineaDto
    {
        /// <summary>
        /// Nombre del usuario que solicita ayuda
        /// </summary>
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string CorreoUsuario { get; set; }
         

        /// <summary>
        /// Asunto del mensaje
        /// </summary>
        public string Asunto { get; set; }

        /// <summary>
        /// Mensaje enviado por el usuario
        /// </summary>
        public string MensajeUsuario { get; set; }
    }

}
