using System.Net.Mail;
using System.Net;
using Hiffy_Servicios.Interfaces;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;

namespace HiffyWebApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;

        }

        public OperationResult EnviarCorreo(EmailRequestDto mensaje)
        {
            try
            {
                var email = new MailMessage();
                string emailPassword = _config.GetSection("EmailUsername").Value;
                string fromPassword = _config.GetSection("EmailPassword").Value;
                email.From = new MailAddress(emailPassword);
                email.To.Add(new MailAddress(mensaje.EmailDestino));
                email.Subject = mensaje.Encabezado;
                email.Body = mensaje.Mensaje;

                email.IsBodyHtml = true;

                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailPassword, fromPassword),
                    EnableSsl = true
                };
                smtp.Send(email);

                return new OperationResult(true, "Exito al enviar el correo", 0);

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message, 0);
            }

        }


    }
}
