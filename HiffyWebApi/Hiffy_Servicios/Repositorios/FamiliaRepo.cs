using Google.Cloud.Translation.V2;
using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static Hiffy_Entidades.Entidades.TareaAsignada;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hiffy_Servicios.Repositorios
{
    public class FamiliaRepo : IFamilia
    {

        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly FirebaseTranslationService _firebasetranslate;


        public FamiliaRepo(AppDbContext context, IEmailService emailService, IConfiguration configuration, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _firebasetranslate = firebasetranslate;

        }
        public async Task<string> GenerarCodigoFamiliaUnicoAsync()
        {
            var random = new Random();
            string codigoFamilia;

            do
            {
                // Genera un código de 8 dígitos
                codigoFamilia = random.Next(10000000, 99999999).ToString();
            }
            while (await _context.Familia.AnyAsync(f => f.CodigoFamilia == codigoFamilia));

            return codigoFamilia;
        }

        public async Task<OperationResult> MostrarFamilia(int IdFamilia, string lenguaje = "es")
        {
            // Fetch the family details by IdFamilia

            var estadoFamiliaPendiente = await _context.EstadoFamilia.FirstOrDefaultAsync(e => e.PendienteFamilia);
            var familia = await _context.Familia
                .Where(f => f.IdFamilia == IdFamilia)
                .Select(f => new FamiliaDto
                {
                    CodigoFamilia = f.CodigoFamilia,
                    Nombre = f.Nombre,
                    Direccion = f.Direccion,
                    Altitud = f.Altitud,
                    Longitud = f.Longitud,
                    FechaCreacion = f.FechaCreacion,
                    miembrosFamiliares = _context.Usuario
                        .Where(u => u.IdFamilia == f.IdFamilia && u.IdEstadoFamilia != estadoFamiliaPendiente.IdEstadoFamilia)
                        .Select(u => new UsuarioDto
                        {
                            IdUsuario = u.IdUsuario,
                            Nombre = u.Nombre,
                            Correo = u.Correo,
                            FechaNacimiento = u.FechaNacimiento,
                            IdRol = u.IdRol,
                            IdFamilia = u.IdFamilia.Value,
                            Rol = u.Rol,
                            RolNombre = _firebasetranslate.Traducir(u.Rol.Nombre, lenguaje),
                            EstadoFamilia = u.EstadoFamilia,
                            EstadoVendedor = u.EstadoVendedor,
                            FotoUrl = u.FotoUrl,
                            RolFamilia = u.RolFamilia,
                            RolFamiliaNombre = _firebasetranslate.Traducir(u.RolFamilia.Nombre, lenguaje),
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            // Check if the family exists
            if (familia == null)
            {
                var mensaje = _firebasetranslate.Traducir("Family not found", lenguaje);

                return new OperationResult(false, mensaje);
            }

            var mensaje2 = _firebasetranslate.Traducir("Success", lenguaje);


            return new OperationResult(true, mensaje2, familia);
        }

        public async Task<OperationResult> ListadoRolesFamiliares(string lenguaje = "es")
        {
            var rolesFamilia = await _context.RolFamilia
            .Select(rol => new RolFamiliaDto
            {
                Descripcion = lenguaje == "es" ? rol.Descripcion : _firebasetranslate.Traducir(rol.Descripcion, lenguaje),
                EsAdmin = rol.EsAdmin,
                IdRolFamilia = rol.IdRolFamilia,
                Nombre = lenguaje == "es" ? rol.Nombre : _firebasetranslate.Traducir(rol.Nombre, lenguaje),
            })
            .ToListAsync();

            var mensaje = _firebasetranslate.Traducir("Roles cargados exitosamente", lenguaje);

            return new OperationResult(true, mensaje, rolesFamilia);
        }

        public async Task<OperationResult> CrearFamilia(PostFamilia dto, int userId, string lenguaje = "es")
        {
            using var transaction = await _context.Database.BeginTransactionAsync(); // Manejo de transacciones
            try
            {
                var codigoFamilia = await GenerarCodigoFamiliaUnicoAsync();
                // Crear nueva familia
                var familia = new Familia
                {
                    CodigoFamilia = codigoFamilia,
                    Nombre = dto.Nombre,
                    Direccion = dto.Direccion,
                    Altitud = dto.Altitud,
                    Longitud = dto.Longitud,
                    FechaCreacion = DateTime.Now
                };

                _context.Familia.Add(familia);
                await _context.SaveChangesAsync();

                // Asociar la familia al usuario
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == userId);
                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                var estadoFamiliaActivo = await _context.EstadoFamilia.FirstOrDefaultAsync(u => u.Activo);

                usuario.IdFamilia = familia.IdFamilia; // Asignar familia al usuario
                usuario.IdRolFamilia = dto.RolFamilia; // Asignar el rol del usuario en la familia
                usuario.IdEstadoFamilia = estadoFamiliaActivo.IdEstadoFamilia; // Asignar el estado familia del usuario a activo
                _context.Usuario.Update(usuario);

                await _context.SaveChangesAsync();

                //Aqui van las areas del hogar
                // Lista para almacenar áreas que no se pudieron crear y sus mensajes
                var areasNoCreadas = new List<object>();

                // Iterar sobre cada elemento de la lista nuevasAreas
                foreach (var nuevaArea in dto.AreasHogar)
                {


                    // Crear una nueva área del hogar asociada a la familia del usuario
                    var areaDelHogar = new AreaDelHogar_Familia
                    {
                        IdFamilia = usuario.IdFamilia.Value,
                        Nombre = nuevaArea.Nombre,
                        Descripcion = nuevaArea.Descripcion,
                        Predeterminado = false,
                        IdEstadoAreasDelHogar = EstadoAreasDelHogar.Activo
                    };

                    // Agregar la nueva área del hogar a la base de datos
                    _context.AreaDelHogar_Familia.Add(areaDelHogar);
                }

                // Guardar todos los cambios en la base de datos
                await _context.SaveChangesAsync();


                await transaction.CommitAsync();

                var mensaje2 = _firebasetranslate.Traducir("Familia creada y usuario actualizado exitosamente", lenguaje);

                return new OperationResult(true, mensaje2, familia);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var mensaje = _firebasetranslate.Traducir("Error al crear la familia", lenguaje);

                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }
        public async Task<OperationResult> ConsultarFamiliaPorCodigo(string codigoFamilia, string lenguaje = "es")
        {
            try
            {
                // Buscar la familia en la base de datos utilizando el código proporcionado
                var familia = await _context.Familia.FirstOrDefaultAsync(f => f.CodigoFamilia == codigoFamilia);

                if (familia == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se encontró una familia con el código proporcionado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }
                var mensaje2 = _firebasetranslate.Traducir("Familia encontrada exitosamente.", lenguaje);

                return new OperationResult(true, mensaje2, familia);
            }
            catch (Exception ex)
            {
                var mensaje2 = _firebasetranslate.Traducir("Error al consultar la familia", lenguaje);

                return new OperationResult(false, $"{mensaje2}: {ex.Message}");
            }
        }
        public async Task<OperationResult> ConsultarFamiliaId(int idFamilia)
        {
            try
            {
                // Buscar la familia en la base de datos utilizando el código proporcionado
                var familia = await _context.Familia.FirstOrDefaultAsync(f => f.IdFamilia == idFamilia);

                if (familia == null)
                {
                    return new OperationResult(false, "No se encontró una familia.");
                }

                return new OperationResult(true, "Familia encontrada exitosamente.", familia);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al consultar la familia: {ex.Message}");
            }
        }


        public async Task<OperationResult> UnirUsuarioAFamilia(int userId, int familiaId, int rolFamiliaId, string lenguaje = "es")
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verificar si la familia existe
                var familia = await _context.Familia.FirstOrDefaultAsync(f => f.IdFamilia == familiaId);
                if (familia == null)
                {
                    var mensaje = _firebasetranslate.Traducir("La familia no existe.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Obtener el usuario
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == userId);
                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                var estadoFamiliaActivo = await _context.EstadoFamilia.FirstOrDefaultAsync(e => e.Activo);
                // Verificar si el usuario ya es activo y tiene familia
                if (usuario.IdFamilia.HasValue && usuario.IdEstadoFamilia == estadoFamiliaActivo.IdEstadoFamilia)
                {
                    var mensaje = _firebasetranslate.Traducir("El usuario ya pertenece a una familia activa.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Asignar la familia al usuario
                usuario.IdFamilia = familiaId;

                // Establecer el rol del usuario en la familia
                usuario.IdRolFamilia = rolFamiliaId;

                // Establecer el estado familia como Pendiente de Validación
                var estadoFamiliaPendiente = await _context.EstadoFamilia.FirstOrDefaultAsync(e => e.PendienteFamilia);
                usuario.IdEstadoFamilia = estadoFamiliaPendiente.IdEstadoFamilia;

                _context.Usuario.Update(usuario);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var mensaje2 = _firebasetranslate.Traducir("Solicitud para unirse a la familia enviada exitosamente.", lenguaje);

                return new OperationResult(true, mensaje2);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var mensaje = _firebasetranslate.Traducir("Error al solicitar unirse a la familia", lenguaje);

                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }

        public async Task<OperationResult> ObtenerUsuariosPendientesFamilia(int idFamilia, string lenguaje = "es")
        {

            // Obtener la lista de usuarios con el estado "Pendiente de Familia" (IdEstadoFamilia = 3) para la familia
            var usuariosPendientes = await _context.Usuario
                .Where(u => u.IdFamilia == idFamilia && u.IdEstadoFamilia == _context.EstadoFamilia.FirstOrDefault(es => es.PendienteFamilia).IdEstadoFamilia)
                .Select(usuario => new UsuarioDto
                {
                    Correo = usuario.Correo,
                    Nombre = usuario.Nombre,
                    FotoUrl = usuario.FotoUrl,
                    IdRol = usuario.IdRol,
                    IdFamilia = usuario.IdFamilia,
                    FechaNacimiento = usuario.FechaNacimiento,
                    RolFamilia = _context.RolFamilia.FirstOrDefault(rf => rf.IdRolFamilia == usuario.IdRolFamilia)
                })
                .ToListAsync();

            return new OperationResult(true, "Usuarios pendientes obtenidos exitosamente.", usuariosPendientes);
        }



        public async Task<OperationResult> AprobarORechazarSolicitud(string correo, bool aprobado, int rolFamiliaId, string lenguaje = "es")
        {
            try
            {
                // Buscar al usuario por correo
                var usuario = await _context.Usuario.Include(u => u.Familia) // Incluir la relación con la tabla Familia
                                                      .FirstOrDefaultAsync(u => u.Correo == correo);

                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                // Buscar el nombre de la familia asociada al usuario (si tiene familia)
                var nombreFamilia = usuario.Familia?.Nombre;

                if (aprobado)
                {
                    var rolUsuario = await _context.RolFamilia.FirstOrDefaultAsync(x => x.IdRolFamilia == rolFamiliaId);
                    var estadoFamiliaActivo = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.Activo);
                    // Cambiar el estado a aprobado
                    usuario.IdEstadoFamilia = estadoFamiliaActivo.IdEstadoFamilia;
                    usuario.IdRolFamilia = rolUsuario.IdRolFamilia;

                    // Crear notificación con el nombre de la familia
                    await _context.Notificacion.AddAsync(new Notificacion
                    {
                        Titulo = _firebasetranslate.Traducir("Solicitud aprobada", lenguaje),
                        Mensaje = _firebasetranslate.Traducir($"Has sido añadido a la familia '{nombreFamilia}' correctamente.", lenguaje),
                        IdUsuarioDestino = usuario.IdUsuario,
                        FechaEnvio = DateTime.Now,
                        Estado = "Enviado"
                    });
                }
                else
                {
                    // Si no está aprobado, limpiar IdRolFamilia e IdFamilia
                    usuario.IdRolFamilia = null;
                    usuario.IdFamilia = null;

                    // Crear notificación indicando que fue rechazado
                    await _context.Notificacion.AddAsync(new Notificacion
                    {
                        Titulo = _firebasetranslate.Traducir("Solicitud rechazada", lenguaje),
                        Mensaje = _firebasetranslate.Traducir("Tu solicitud para unirte a la familia fue rechazada.", lenguaje),
                        IdUsuarioDestino = usuario.IdUsuario,
                        FechaEnvio = DateTime.Now,
                        Estado = "Enviado"
                    });
                }

                // Guardar cambios en la base de datos
                await _context.SaveChangesAsync();

                // Mensajes de éxito traducidos
                var mensajeExito = aprobado
                    ? _firebasetranslate.Traducir($"La solicitud fue aprobada y el usuario añadido a la familia '{nombreFamilia}'.", lenguaje)
                    : _firebasetranslate.Traducir("La solicitud fue rechazada y el usuario fue eliminado de la familia.", lenguaje);

                return new OperationResult(true, mensajeExito);
            }
            catch (Exception ex)
            {
                var mensajeError = _firebasetranslate.Traducir("Error al solicitar unirse a la familia", lenguaje);
                return new OperationResult(false, $"{mensajeError}: {ex.Message}");
            }
        }


        public async Task<OperationResult> ActualizarUsuario(ActualizarUsuarioFamiliaDto actualizarUsuario, int usuarioAdmin, string lenguaje = "es")
        {
            try
            {
                var usuarioAdminFamiliar = await _context.Usuario.Include(x => x.RolFamilia).FirstOrDefaultAsync(x => x.IdUsuario == usuarioAdmin);
                if (usuarioAdminFamiliar == null)
                {
                    var mensajeError = _firebasetranslate.Traducir("Error al validar usuario.", lenguaje);
                    return new OperationResult(false, mensajeError);
                }

                if (usuarioAdminFamiliar.RolFamilia.EsAdmin)
                {
                    // Llamar al repositorio para actualizar el usuario
                    var usuario = await _context.Usuario
                        .Where(x => x.IdUsuario == actualizarUsuario.IdUsuario)
                        .FirstOrDefaultAsync();

                    if (usuario == null)
                    {
                        var mensajeError = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);
                        return new OperationResult(false, mensajeError);
                    }

                    // Validar si el correo es diferente y ya está registrado en la base de datos
                    if (!string.Equals(usuario.Correo, actualizarUsuario.Correo, StringComparison.OrdinalIgnoreCase))
                    {
                        var existeCorreo = await _context.Usuario
                            .AnyAsync(x => x.Correo == actualizarUsuario.Correo && x.IdUsuario != actualizarUsuario.IdUsuario);
                        if (existeCorreo)
                        {
                            var mensajeError = _firebasetranslate.Traducir("El correo proporcionado ya está en uso por otro usuario.", lenguaje);
                            return new OperationResult(false, mensajeError);
                        }
                    }

                    var edad = DateTime.Now.Year - actualizarUsuario.FechaNacimiento.Year;
                    if (actualizarUsuario.FechaNacimiento > DateTime.Now.AddYears(-edad))
                    {
                        edad--;
                    }

                    // Validar si es mayor de edad y tiene documento
                    if (edad >= 18)
                    {
                        if (actualizarUsuario.IdTipoDocumento == null || string.IsNullOrWhiteSpace(actualizarUsuario.Documento))
                        {
                            var mensajeError = _firebasetranslate.Traducir("Los usuarios mayores de edad deben tener un tipo de documento y un documento válido.", lenguaje);
                            return new OperationResult(false, mensajeError, 0);
                        }
                    }

                    // Actualizar las propiedades del usuario
                    usuario.Nombre = actualizarUsuario.Nombre;
                    usuario.Correo = actualizarUsuario.Correo;
                    usuario.FechaNacimiento = actualizarUsuario.FechaNacimiento;
                    usuario.Sexo = actualizarUsuario.Sexo;
                    usuario.IdRolFamilia = actualizarUsuario.IdRolFamilia;
                    usuario.IdTipoDocumento = edad >= 18 ? actualizarUsuario.IdTipoDocumento : null;
                    usuario.Documento = edad >= 18 ? actualizarUsuario.Documento : "";

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    var mensajeExito = _firebasetranslate.Traducir("Actualización exitosa.", lenguaje);
                    return new OperationResult(true, mensajeExito, usuario.Nombre);
                }
                else
                {
                    var mensajeError = _firebasetranslate.Traducir("No posee permisos para realizar esta acción.", lenguaje);
                    return new OperationResult(false, mensajeError);
                }
            }
            catch (Exception ex)
            {
                var mensajeError = _firebasetranslate.Traducir("Error al actualizar el usuario.", lenguaje);
                return new OperationResult(false, $"{mensajeError}: {ex.Message}");
            }
        }


        public async Task<OperationResult> ObtenerDashboard(int idFamilia, int idUsuario, string lenguaje = "es")
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var hoyInicio = DateTime.Now.Date; // Inicio del día a las 00:00
            var hoyFin = hoyInicio.AddDays(1).AddSeconds(-1); // Fin del día a las 23:59:59

            var haceUnaSemana = hoyInicio.AddDays(-7);

            // Total de tareas del mes
            var serviciosTotalesMes = await _context.ContratoPersonal
                .Where(t => t.FechaInicio >= inicioMes && t.FechaInicio < inicioMes.AddMonths(1) && t.IdFamilia == idFamilia)
                .ToListAsync();

            // Total de tareas del mes
            var tareasCompletas = await _context.TareaAsignada
                .Where(t => t.FechaInicio >= inicioMes
                    && t.FechaInicio < inicioMes.AddMonths(1)
                    && t.AreaFamilia.IdFamilia == idFamilia)
                .Select(tarea => new TareaAsignadaDashBoard
                {
                    IdTareaAsignada = tarea.IdTareaAsignada,
                    IdUsuario = tarea.IdUsuario,
                    IdTareaDomestica = tarea.IdTareaDomestica,
                    IdAreaFamilia = tarea.IdAreaFamilia,
                    FechaInicio = tarea.FechaInicio,
                    FechaFin = tarea.FechaFin,
                    HoraInicio = tarea.HoraInicio,
                    HoraFin = tarea.HoraFin,
                    Prioridad = tarea.Prioridad,
                    EsRecurrente = tarea.EsRecurrente,
                    Estado = tarea.Estado
                })
                .ToListAsync();

            // Generar las tareas recurrentes en memoria
            var tareasRecurrentes = tareasCompletas
            .Where(t => t.EsRecurrente)
            .SelectMany(tarea => _context.RecurrenciaTareas
                .Where(r => r.IdTareaAsignada == tarea.IdTareaAsignada && r.FechaDia >= inicioMes && r.FechaDia < inicioMes.AddMonths(1)) // Filtrar por mes actual
                .Select(recurrencia => new TareaAsignadaDashBoard
                {
                    IdTareaAsignada = tarea.IdTareaAsignada,
                    IdUsuario = tarea.IdUsuario,
                    IdTareaDomestica = tarea.IdTareaDomestica,
                    IdAreaFamilia = tarea.IdAreaFamilia,
                    FechaInicio = recurrencia.FechaDia,
                    FechaFin = tarea.FechaFin,
                    HoraInicio = tarea.HoraInicio,
                    HoraFin = tarea.HoraFin,
                    Prioridad = tarea.Prioridad,
                    EsRecurrente = tarea.EsRecurrente,
                    Estado = recurrencia.Estado
                }))
            .ToList();


            // Filtrar las tareas originales que no son recurrentes
            var tareasNoRecurrentes = tareasCompletas
                .Where(t => !t.EsRecurrente)
                .ToList();

            // Combinar las tareas no recurrentes con las recurrentes
            var tareasTotalesMes = tareasNoRecurrentes.Concat(tareasRecurrentes).ToList();



            // Tareas para hoy
            var tareasHoy = tareasTotalesMes.Where(t => t.FechaInicio >= hoyInicio && t.FechaInicio <= hoyFin).ToList();



            // Tareas pendientes del mes
            var tareasPendientesMes = tareasTotalesMes.Where(t => t.Estado == EstadoTarea.Pendiente).ToList();
            var tareasPendientesHoy = tareasPendientesMes
            .Where(t => t.FechaInicio >= hoyInicio && t.FechaInicio <= hoyFin)
            .ToList();

            // Tareas completadas del mes
            var tareasCompletadasMes = tareasTotalesMes.Where(t => t.Estado == EstadoTarea.Completada).ToList();
            var tareasCompletadasSemana = tareasCompletadasMes.Where(t => t.FechaFin >= haceUnaSemana).ToList();

            // Servicios pendientes del mes
            var serviciosPendientesMes = serviciosTotalesMes.Where(t => t.Estado == EstadoContrato.Aceptado || t.Estado == EstadoContrato.EnCurso).ToList();

            // Servicios cerrados de la semana
            var serviciosCerradosSemana = serviciosTotalesMes.Where(t => t.Estado == EstadoContrato.Finalizado && t.FechaFin >= haceUnaSemana).ToList();

            // Total de tareas por semana en el mes
            var tareasPorSemana = tareasTotalesMes
            .Where(t => t.FechaInicio >= inicioMes && t.FechaInicio < inicioMes.AddMonths(1)) // Filtrar tareas dentro del mes actual
            .GroupBy(t => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(t.FechaInicio, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
            .Select(g => new
            {
                Semana = g.Key,
                Total = g.Count(),
                Completadas = g.Count(t => t.Estado == EstadoTarea.Completada),
                Pendientes = g.Count(t => t.Estado == EstadoTarea.Pendiente)
            }).ToList();


            // Obtener los IDs únicos de usuarios en tareasTotalesMes
            var usuarioIds = tareasTotalesMes
                .Select(t => t.IdUsuario)
                .Distinct()
                .ToList();

            // Consultar los nombres de los usuarios desde la base de datos
            var usuarios = await _context.Usuario
                .Where(u => usuarioIds.Contains(u.IdUsuario))
                .Select(u => new { u.IdUsuario, u.Nombre })
                .ToListAsync();

            // Top 2 usuarios con más tareas completadas en el mes
            var topUsuarios = tareasTotalesMes
                .Where(t => t.Estado == EstadoTarea.Completada) // Solo tareas completadas
                .GroupBy(t => t.IdUsuario) // Agrupar por usuario
                .Select(g => new
                {
                    Usuario = g.Key,
                    Nombre = usuarios.FirstOrDefault(u => u.IdUsuario == g.Key)?.Nombre ?? "Desconocido",
                    TotalCompletadas = g.Count()
                })
                .OrderByDescending(u => u.TotalCompletadas) // Ordenar por total de tareas completadas
                .Take(2) // Tomar los dos primeros
                .ToList();

            var topUsuariosIds = topUsuarios.Select(u => u.Usuario).ToList();

            // Consultamos las fotos de estos usuarios
            var usuariosConFotos = await _context.Usuario
                .Where(u => topUsuariosIds.Contains(u.IdUsuario))
                .Select(u => new { u.IdUsuario, u.FotoUrl })
                .ToListAsync();

            // Creamos el resultado final combinando la información
            var topUsuariosConFotos = topUsuarios.Select(u => new
            {
                Usuario = u.Usuario,
                Nombre = u.Nombre,
                FotoUrl = usuariosConFotos.FirstOrDefault(f => f.IdUsuario == u.Usuario)?.FotoUrl ?? "SinFoto",
                TotalCompletadas = u.TotalCompletadas
            }).ToList();


            // Obtener las contrataciones del mes para la familia
            // Obtener las contrataciones del mes con los 2 mejores vendedores
            var contrataciones = await _context.ContratoPersonal
                .Where(t => t.FechaInicio >= inicioMes && t.FechaInicio < inicioMes.AddMonths(1) && t.IdFamilia == idFamilia && t.Valoracion > 0)
                .GroupBy(c => c.Servicio.IdUsuario)
                .Select(grupo => new
                {
                    IdUsuario = grupo.Key,
                    NombreVendedor = grupo.FirstOrDefault().Servicio.Usuario.Nombre,
                    PromedioValoracion = grupo.Average(c => c.Valoracion)
                })
                .OrderByDescending(v => v.PromedioValoracion)
                .Take(2)
                .ToListAsync();


            var mensaje = _firebasetranslate.Traducir("Datos obtenidos exitosamente", lenguaje);

            // Obtener las fotos de los usuarios (vendedores) del top 2
            var idsUsuarios = contrataciones.Select(c => c.IdUsuario).ToList();
            var fotosUsuarios = await _context.Usuario
                .Where(u => idsUsuarios.Contains(u.IdUsuario))
                .Select(u => new { u.IdUsuario, u.FotoUrl })
                .ToListAsync();

            // Agregar la FotoUrl al resultado final
            var contratacionesConFoto = contrataciones
                .Select(c => new
                {
                    c.IdUsuario,
                    c.NombreVendedor,
                    c.PromedioValoracion,
                    FotoUrl = fotosUsuarios.FirstOrDefault(f => f.IdUsuario == c.IdUsuario)?.FotoUrl ?? "SinFoto"
                })
                .ToList();





            return new OperationResult(true, mensaje, new
            {
                TareasTotalesMes = tareasTotalesMes.Count,
                TareasHoy = tareasHoy.Count,
                TareasPendientesMes = tareasPendientesMes.Count,
                TareasPendientesHoy = tareasPendientesHoy.Count,
                TareasCompletadasMes = tareasCompletadasMes.Count,
                TareasCompletadasSemana = tareasCompletadasSemana.Count,
                ServiciosPendientesMes = serviciosPendientesMes.Count,
                ServiciosCerradosSemana = serviciosCerradosSemana.Count,
                TareasPorSemana = tareasPorSemana,
                TopUsuarios = topUsuariosConFotos,
                TopContrataciones = contratacionesConFoto
            });
        }

        public async Task<OperationResult> ObtenerDashboardVendedor(int idUsuario, string lenguaje = "es")
        {
            try
            {
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var inicioSemana = DateTime.Now.AddDays(-7).Date;
                var hoy = DateTime.Now.Date;

                // Contratos personales pendientes del mes
                var contratosPendientesMes = await _context.ContratoPersonal
                    .Where(c => c.Servicio.IdUsuario == idUsuario
                                && c.Estado == EstadoContrato.Aceptado || c.Estado == EstadoContrato.EnCurso
                                && c.FechaInicio >= inicioMes
                                && c.FechaInicio < inicioMes.AddMonths(1))
                    .ToListAsync();

                // Contratos personales pendientes del mes para hoy
                var contratosPendientesHoy = contratosPendientesMes
                    .Count(c => c.FechaInicio.Date == hoy);

                // Contratos personales completados en el mes
                var contratosCompletadosMes = await _context.ContratoPersonal
                    .Where(c => c.Servicio.IdUsuario == idUsuario
                                && c.Estado == EstadoContrato.Finalizado
                                && c.FechaInicio >= inicioMes
                                && c.FechaInicio < inicioMes.AddMonths(1))
                    .ToListAsync();

                // Contratos completados en la última semana
                var contratosCompletadosSemana = contratosCompletadosMes
                    .Count(c => c.FechaInicio.Date >= inicioSemana);

                // Contratos por semana en el mes
                var contratosPorSemana = contratosCompletadosMes
                    .Concat(contratosPendientesMes)
                    .GroupBy(c => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        c.FechaInicio,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday))
                    .Select(g => new
                    {
                        Semana = g.Key,
                        Total = g.Count(),
                        Completados = g.Count(c => c.Estado == EstadoContrato.Finalizado),
                        Pendientes = g.Count(c => c.Estado == EstadoContrato.Aceptado || c.Estado == EstadoContrato.EnCurso)
                    })
                    .OrderBy(g => g.Semana)
                    .ToList();

                // Obtener la cantidad de servicios por tipo para el usuario
                var serviciosPorTipo = await _context.Servicio
                    .Where(s => s.IdUsuario == idUsuario) // Filtrar por el usuario solicitado
                    .GroupBy(s => s.IdTipoServicio) // Agrupar por tipo de servicio
                    .Select(g => new
                    {
                        TipoServicio = g.FirstOrDefault().TipoServicio.Nombre, // El tipo de servicio
                        TotalServicios = g.Count() // Cantidad de servicios en este tipo
                    })
                    .ToListAsync();

                var mensaje = _firebasetranslate.Traducir("Dashboard de vendedor obtenido con éxito.", lenguaje);


                return new OperationResult(true, mensaje, new
                {
                    ContratosPendientesMes = contratosPendientesMes.Count,
                    ContratosPendientesHoy = contratosPendientesHoy,
                    ContratosCompletadosMes = contratosCompletadosMes.Count,
                    ContratosCompletadosSemana = contratosCompletadosSemana,
                    ContratosPorSemana = contratosPorSemana,
                    ServiciosPorTipo = serviciosPorTipo
                });
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        public async Task<OperationResult> ObtenerDashboardAdmin(string lenguaje = "es")
        {
            try
            {
                var familias = await _context.Familia.ToListAsync();



                var usuariosRolDual = await _context.Usuario
                .CountAsync(u => u.IdRol == 4);

                // Obtener cantidad de usuarios por rol, excluyendo inicialmente el rol dual
                var usuariosPorRol = await _context.Usuario
                    .Where(a => a.IdRol != 4)
                    .GroupBy(u => u.IdRol)
                    .Select(g => new
                    {
                        Rol = g.FirstOrDefault().Rol.Nombre,
                        Total = g.Count()
                    })
                    .ToListAsync();

                // Convertir a una lista que podamos modificar
                var usuariosPorRolAjustado = usuariosPorRol.Select(u => new
                {
                    Rol = u.Rol,
                    Total = u.Rol == "Vendedor" ? u.Total + usuariosRolDual :
                           u.Rol == "Familiar" ? u.Total + usuariosRolDual :
                           u.Total
                }).ToList();

                // Obtener estados de vendedores
                var estadosVendedores = await _context.Usuario
                    .Where(u => u.IdRol == 2 || u.IdRol == 4) // 2 = Rol Vendedor
                    .GroupBy(u => u.IdEstadoVendedor)
                    .Select(g => new
                    {
                        Estado = g.FirstOrDefault().EstadoVendedor.Descripcion,
                        Total = g.Count()
                    })
                    .ToListAsync();

                // Obtener estados de familias
                var estadosFamilias = await _context.Usuario
                    .Where(u => u.IdRol == 3 || u.IdRol == 4) // 3 = Rol Familiar
                    .GroupBy(u => u.IdEstadoFamilia)
                    .Select(g => new
                    {
                        Estado = g.FirstOrDefault().EstadoFamilia.Descripcion,
                        Total = g.Count()
                    })
                    .ToListAsync();

                // Crear resumen detallado de estados por tipo de usuario
                var resumenEstados = new
                {
                    Vendedores = new
                    {
                        Activos = estadosVendedores.FirstOrDefault(e => e.Estado == "Activo")?.Total ?? 0,
                        Inactivos = estadosVendedores.FirstOrDefault(e => e.Estado == "Inactivo")?.Total ?? 0,
                        Suspendidos = estadosVendedores.FirstOrDefault(e => e.Estado == "Suspendida")?.Total ?? 0,
                        PendientesValidacion = estadosVendedores.FirstOrDefault(e => e.Estado == "Pendiente Validación")?.Total ?? 0
                    },
                    Familias = new
                    {
                        Activas = estadosFamilias.FirstOrDefault(e => e.Estado == "Activo")?.Total ?? 0,
                        Inactivas = estadosFamilias.FirstOrDefault(e => e.Estado == "Inactivo")?.Total ?? 0,
                        Suspendidas = estadosFamilias.FirstOrDefault(e => e.Estado == "Suspendida")?.Total ?? 0,
                        Pendientes = estadosFamilias.FirstOrDefault(e => e.Estado == "Pendiente")?.Total ?? 0
                    }
                };

                // Obtener certificaciones pendientes
                var certificacionesPendientes = await _context.CertificacionVendedor
                    .Where(cv => !_context.CertificacionTipoServicio
                        .Any(cts => cts.IdCertificacion == cv.IdCertificacion))
                    .CountAsync();

                // Obtener distribución de vendedores por tipo de servicio
                var vendedoresPorTipoServicio = await _context.Servicio
                    .Where(s => s.Usuario.IdRol == 2 || s.Usuario.IdRol == 4) // Incluye vendedores y roles duales
                    .GroupBy(s => s.IdTipoServicio)
                    .Select(g => new
                    {
                        TipoServicio = g.FirstOrDefault().TipoServicio.Nombre,
                        TotalVendedores = g.Select(s => s.IdUsuario).Distinct().Count()
                    })
                    .ToListAsync();

                var mensaje = _firebasetranslate.Traducir("Dashboard de administrador obtenido con éxito.", lenguaje);

                return new OperationResult(true, mensaje, new
                {
                    TotalFamilias = familias.Count,

                    UsuariosPorRol = usuariosPorRolAjustado,
                    ResumenEstados = resumenEstados,
                    CertificacionesPendientes = certificacionesPendientes,
                    VendedoresPorTipoServicio = vendedoresPorTipoServicio

                });
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        public async Task<OperationResult> RemoverUsuarioDeFamilia(int idUsuario, int? idUsuarioSolicitante = null)
        {
            // Buscar el usuario en la base de datos
            var usuario = await _context.Usuario
                .Include(u => u.Familia)
                .Include(u => u.RolFamilia) // Incluye el rol de la familia
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return new OperationResult(false, "El usuario no existe.");
            }

            // Verificar si el usuario pertenece a una familia
            if (usuario.IdFamilia == null)
            {
                return new OperationResult(false, "El usuario no pertenece a ninguna familia.");
            }

            // Validar si el usuario está intentando eliminarse a sí mismo
            bool esAutoEliminacion = idUsuarioSolicitante.HasValue && idUsuarioSolicitante.Value == idUsuario;

            // Si el usuario está abandonando la familia él mismo
            if (esAutoEliminacion)
            {
                // Verificar si es el único administrador
                var esAdministrador = usuario.RolFamilia != null && usuario.RolFamilia.EsAdmin;
                if (esAdministrador)
                {
                    var otroAdmin = await _context.Usuario
                        .Include(u => u.RolFamilia)
                        .Where(u => u.IdFamilia == usuario.IdFamilia && u.IdUsuario != idUsuario && u.RolFamilia.EsAdmin)
                        .FirstOrDefaultAsync();

                    if (otroAdmin == null)
                    {
                        return new OperationResult(false, "No puedes abandonar la familia porque eres el único administrador.");
                    }
                }
            }
            else
            {
                // Validar si el solicitante tiene permisos para eliminar al usuario
                if (!idUsuarioSolicitante.HasValue)
                {
                    return new OperationResult(false, "No tienes permiso para realizar esta acción.");
                }

                var solicitante = await _context.Usuario
                    .Include(u => u.RolFamilia)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuarioSolicitante.Value);

                if (solicitante == null || solicitante.IdFamilia != usuario.IdFamilia || solicitante.RolFamilia == null || !solicitante.RolFamilia.EsAdmin)
                {
                    return new OperationResult(false, "No tienes permiso para eliminar a este usuario.");
                }
            }

            // Buscar y preparar las tareas asignadas
            var tareasAsignadas = await _context.TareaAsignada
                .Where(t => t.IdUsuario == idUsuario)
                .Include(t => t.Recurrencias)
                .ToListAsync();

            if (tareasAsignadas.Any())
            {
                // Eliminar las recurrencias asociadas a las tareas asignadas
                foreach (var tareaAsignada in tareasAsignadas)
                {
                    if (tareaAsignada.Recurrencias != null && tareaAsignada.Recurrencias.Any())
                    {
                        _context.RecurrenciaTareas.RemoveRange(tareaAsignada.Recurrencias);
                    }

                    // Eliminar la tarea asignada
                    _context.TareaAsignada.Remove(tareaAsignada);
                }
            }

            // Eliminar la relación del usuario con la familia
            usuario.IdFamilia = null;
            usuario.IdRolFamilia = null; // Opcional, según el modelo
            usuario.IdEstadoFamilia = await _context.EstadoFamilia
                .Where(e => e.PendienteFamilia)
                .Select(e => e.IdEstadoFamilia)
                .FirstOrDefaultAsync();

            if (usuario.IdEstadoFamilia == 0)
            {
                return new OperationResult(false, "No se encontró el estado pendiente para el usuario.");
            }

            // Guardar los cambios
            _context.Usuario.Update(usuario);
            await _context.SaveChangesAsync();

            return new OperationResult(true, "El usuario ha sido removido de la familia, sus tareas han sido eliminadas y las recurrencias han sido removidas exitosamente.");
        }


        public async Task<OperationResult> ActualizarUbicacionHogar(int userId, string latitud, string longitud, string lenguaje = "es")

        {

            try
            {
                var usuario = await _context.Usuario.Include(u => u.RolFamilia).FirstOrDefaultAsync(u => u.IdUsuario == userId);
                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }
                if (!usuario.RolFamilia.EsAdmin)
                {
                    var mensaje = _firebasetranslate.Traducir("No posee permisos para cambiar la ubicación del hogar", lenguaje);

                    return new OperationResult(false, $"{mensaje}");
                }

                var familia = await _context.Familia.FirstOrDefaultAsync(f => f.IdFamilia == usuario.IdFamilia);
                if (familia == null)
                {
                    var mensaje = _firebasetranslate.Traducir("La familia no existe.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                familia.Longitud = longitud;
                familia.Altitud = latitud;
                await _context.SaveChangesAsync();

                var mensaje2 = _firebasetranslate.Traducir("Actualización exitosa", lenguaje);

                return new OperationResult(true, $"{mensaje2}");
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error al actualizar la ubicación del hogar", lenguaje);

                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }
    }
}