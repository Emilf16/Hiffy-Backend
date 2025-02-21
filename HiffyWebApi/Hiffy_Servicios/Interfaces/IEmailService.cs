using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Interfaces
{
    public interface IEmailService
    {
        OperationResult EnviarCorreo(EmailRequestDto correo);
    }
}
