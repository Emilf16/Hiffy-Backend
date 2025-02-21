using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Dtos
{
    public class ReseteoClaveDto
    {
        [Required]
        public string Correo { get; set; }
        [Required]
        public string Codigo { get; set; }
        [Required, MinLength(6, ErrorMessage = "Favor introducir una clave con al menos 6 caracteres")]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
