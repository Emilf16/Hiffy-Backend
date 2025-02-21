
using Hiffy_Servicios.Interfaces;
using Hiffy_Servicios.Repositorios;
using Hiffy_Servicios.Servicios;

namespace HiffyWebApi.Services
{
    public static class RepositorioServices
    {
        public static IServiceCollection AddRepositorios(this IServiceCollection services)
        {
            return services
                .AddScoped<FamiliaRepo>()
                .AddScoped<UsuarioRepo>()
                .AddScoped<UsuarioRepo>()
                .AddScoped<VendedorRepo>()
                .AddScoped<AreaDelHogarRepo>()
                .AddScoped<AsistenteDeVozRepo>()
                .AddScoped<TareaDomesticaRepo>()
                .AddScoped<TareaAsignadaRepo>()
                .AddScoped<TipoServicioRepositorio>()
                .AddScoped<ServicioRepositorio>()
                .AddScoped<ContratoPersonalRepositorio>()
                .AddScoped<IEmailService, EmailService>()
                .AddScoped<NotificacionRepositorio>()
                .AddScoped<INotificationService, NotificationService>()
                .AddScoped<FirebaseTranslationService>();
        }
    }
}
