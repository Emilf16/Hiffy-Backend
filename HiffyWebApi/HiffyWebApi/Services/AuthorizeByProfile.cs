using Hiffy_Servicios.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Hiffy_Servicios.Enum;
using Microsoft.AspNetCore.Mvc;


namespace HiffyWebApi.Services
{
    public sealed class AuthorizeByProfile : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Los perfiles requeridos para acceder al recurso.
        /// </summary>
        public PerfilesEnum[] Perfiles { get; set; }

        /// <summary>
        /// Indica si se permite cualquier rol para acceder al recurso.
        /// </summary>
        public bool AllowAnyRole { get; set; }

        /// <summary>
        /// Indica si el usuario está autenticado.
        /// </summary>
        public bool IsAuthenticated;

        private DbContext _DbContext;
        private PerfilesEnum admin;
        private readonly PerfilesEnum[] _perfiles;
        private readonly bool _allowAnyRole;
        private readonly UsuarioRepo _usuarioRepo;

        /// <summary>
        /// Constructor de la clase AuthorizeByPermission.
        /// </summary>
        /// <param name="perfiles">Los perfiles requeridos para acceder al recurso.</param>
        public AuthorizeByProfile(UsuarioRepo usuarioRepo, bool allowAnyRole = false, params PerfilesEnum[] perfiles)
        {
            _usuarioRepo = usuarioRepo;
            _allowAnyRole = allowAnyRole;
            _perfiles = perfiles;
        }

        public AuthorizeByProfile(PerfilesEnum admin)
        {
            this.admin = admin;
        }

        /// <summary>
        /// Método que se ejecuta durante la autorización y verifica si el usuario está autorizado para acceder al recurso.
        /// </summary>
        /// <param name="context">El contexto de autorización.</param>
        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!await IsAuthorizedAsync(context))
            {
                HandleUnauthorizedRequest(context);
            }
        }

        /// <summary>
        /// Verifica si el usuario está autorizado para acceder al recurso.
        /// </summary>
        /// <param name="context">El contexto de autorización.</param>
        /// <returns>True si el usuario está autorizado, False en caso contrario.</returns>
        private async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext context)
        {
            if (context != null)
            {
                var User = context.HttpContext?.User;

                if (User == null || !User.Identity.IsAuthenticated)
                {
                    IsAuthenticated = false;
                    return false;
                }

                IsAuthenticated = true;

                if (AllowAnyRole) return true;

                var configuration = context?.HttpContext?.RequestServices.GetService<IConfiguration>();
                var connectionString = configuration?.GetConnectionString("Conexion");

                var contextOptions = new DbContextOptionsBuilder<DbContext>().UseSqlServer(connectionString).Options;

                _DbContext = new DbContext(contextOptions);
                //int idUsuario = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "idUsuario")?.Value);
                int idUsuario = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario));

               
                    var usuario = await _usuarioRepo.ObtenerUsuarioPorIdV2(idUsuario);

                    if (usuario == null) return false;

                    if (Perfiles != null && Perfiles.Length > 0)
                    {
                        foreach (var perfil in Perfiles)
                        {
                            if (usuario.IdRol == (int)perfil) return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
               
            }
            return false;
        }

        /// <summary>
        /// Maneja las solicitudes no autorizadas.
        /// </summary>
        /// <param name="context">El contexto de autorización.</param>
        private void HandleUnauthorizedRequest(AuthorizationFilterContext context)
        {
            if (context != null)
            {
                var User = context.HttpContext?.User;

                if (User == null || !User.Identity.IsAuthenticated)
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
