using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hiffy_Entidades.Entidades.TareaAsignada;

namespace Hiffy_Servicios.Repositorios
{
    public class TareaAsignadaRepo
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly FirebaseTranslationService _firebasetranslate;

        public TareaAsignadaRepo(AppDbContext context, INotificationService notificationService, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _notificationService = notificationService;
            _firebasetranslate = firebasetranslate;

        }
        public async Task<OperationResult> CrearTareaAsignada(int idUsuario, TareaAsignadaDto tareaAsignadaDto, string lenguaje = "es")
        {
            try
            {
                // Verificar que el usuario existe
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Crear una nueva tarea asignada
                var tareaAsignada = new TareaAsignada
                {
                    IdUsuario = tareaAsignadaDto.IdUsuario,
                    IdTareaDomestica = tareaAsignadaDto.IdTareaDomestica,
                    IdAreaFamilia = tareaAsignadaDto.IdAreaFamilia,
                    Descripcion = tareaAsignadaDto.Descripcion,
                    FechaInicio = tareaAsignadaDto.FechaInicio,
                    FechaFin = tareaAsignadaDto.FechaFin,
                    Prioridad = tareaAsignadaDto.Prioridad,
                    Estado = tareaAsignadaDto.Estado,
                    //SI LA TAREA ES RECURRENTE SE INDICA EL DIA Y LA HORA QUE MOSTRARAN LOS DIAS QUE SE HARA
                    EsRecurrente = tareaAsignadaDto.EsRecurrente,
                    DiaSemana = tareaAsignadaDto.EsRecurrente ? tareaAsignadaDto.DiaSemana: null,
                    HoraInicio = tareaAsignadaDto.HoraInicio,
                    HoraFin = tareaAsignadaDto.HoraFin,
                };
                // Guardar la tarea asignada en la base de datos
                _context.TareaAsignada.Add(tareaAsignada);
                await _context.SaveChangesAsync();

                var tareaDomestica = await _context.TareaDomestica
            .FirstOrDefaultAsync(t => t.IdTareaDomestica == tareaAsignadaDto.IdTareaDomestica);

                var Area = await _context.AreaDelHogar_Familia
           .FirstOrDefaultAsync(t => t.IdAreaFamilia == tareaAsignadaDto.IdAreaFamilia);

                int totalRecurrencias = 0;

                if (tareaAsignadaDto.EsRecurrente)
                {
                    // Ajustar la fecha de inicio al primer día que coincida con el día de la semana deseado
                    DateTime fechaActual = tareaAsignadaDto.FechaInicio.Date;

                    // Ajustar al próximo día de la semana deseado si no coincide
                    while (fechaActual.DayOfWeek != (DayOfWeek)(((int)tareaAsignadaDto.DiaSemana % 7)))
                    {
                        fechaActual = fechaActual.AddDays(1);
                    }

                    // Iterar sumando 7 días en cada paso
                    while (fechaActual <= tareaAsignadaDto.FechaFin.Date)
                    {
                        var tareaRecurrente = new RecurrenciaTareas
                        {
                            IdTareaAsignada = tareaAsignada.IdTareaAsignada,
                            Estado = tareaAsignadaDto.Estado,
                            FechaDia = fechaActual.Date, // Fecha del día encontrado 
                        };

                        // Guardar la tarea recurrente en la base de datos
                        _context.RecurrenciaTareas.Add(tareaRecurrente);
                        totalRecurrencias++;
                        // Avanzar 7 días para encontrar el siguiente día de la semana
                        fechaActual = fechaActual.AddDays(7);
                    }
                } 
                await _context.SaveChangesAsync();

                string resumenNotificacion;
                if (tareaAsignadaDto.EsRecurrente)
                {
                    resumenNotificacion = $@"
                    Se te ha asignado la tarea de {tareaDomestica.Nombre} en el área: {Area.Nombre}.
                    Esta es una tarea recurrente que deberás realizar {totalRecurrencias} veces:
                    - Cada {(DiaSemana)tareaAsignadaDto.DiaSemana}
                    - Desde: {tareaAsignada.FechaInicio:dd/MM/yyyy}
                    - Hasta: {tareaAsignada.FechaFin:dd/MM/yyyy}
                    - Horario: {tareaAsignada.HoraInicio:HH:mm} - {tareaAsignada.HoraFin:HH:mm}";
                }
                else
                {
                                        resumenNotificacion = $@"
                    Se te ha asignado la tarea de {tareaDomestica.Nombre} en el área: {Area.Nombre}, 
                    para empezar el {tareaAsignada.FechaInicio:dd/MM/yyyy}.";
                }

                await _notificationService.SendNotificationAsync(
                    "Nueva tarea asignada",
                    resumenNotificacion,
                    tareaAsignadaDto.IdUsuario);

                var mensaje2 = _firebasetranslate.Traducir("Tarea asignada creada exitosamente.", lenguaje);

                return new OperationResult(true, mensaje2);
            }

            catch (DbUpdateException ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error al crear la tarea asignada", lenguaje);

                // Captura la excepción interna y devuelve el mensaje detallado
                return new OperationResult(false, $"{mensaje}: {ex.InnerException?.Message}");
            }

        }
        public async Task<OperationResult> ObtenerTareaAsignadaPorId(int idTareaAsignada, string lenguaje = "es")
        {
            // Buscar la tarea asignada por ID
            var tareaAsignada = await _context.TareaAsignada
                .FirstOrDefaultAsync(t => t.IdTareaAsignada == idTareaAsignada);

            if (tareaAsignada == null)
            {
                var mensaje = _firebasetranslate.Traducir("Tarea asignada no encontrada.", lenguaje);

                return new OperationResult(false, mensaje, null);
            }

            // Mapear la entidad a DTO
            var tareaAsignadaDto = new TareaAsignadaDto
            {
                IdTareaAsignada = tareaAsignada.IdTareaAsignada,
                IdUsuario = tareaAsignada.IdUsuario,
                IdTareaDomestica = tareaAsignada.IdTareaDomestica,
                IdAreaFamilia = tareaAsignada.IdAreaFamilia,
                Descripcion = tareaAsignada.Descripcion,
                FechaInicio = tareaAsignada.FechaInicio,
                FechaFin = tareaAsignada.FechaFin,
                Prioridad = tareaAsignada.Prioridad,
                Estado = tareaAsignada.Estado,
                EsRecurrente = tareaAsignada.EsRecurrente, 
                DiaSemana = tareaAsignada.EsRecurrente ? tareaAsignada.DiaSemana : null,
                HoraInicio = tareaAsignada.HoraInicio,
                HoraFin = tareaAsignada.HoraFin,
            };

            // Si la tarea es recurrente, obtener el detalle de la recurrencia
            if (tareaAsignada.EsRecurrente)
            {
                //var recurrencia = await _context.RecurrenciaTareas
                //    .FirstOrDefaultAsync(r => r.IdTareaAsignada == tareaAsignada.IdTareaAsignada);

                //if (recurrencia != null)
                //{ 




                //}
            }
            var mensaje2 = _firebasetranslate.Traducir("Tarea asignada encontrada.", lenguaje);

            return new OperationResult(true, mensaje2, tareaAsignadaDto);
        }
        public async Task<OperationResult> CompletarTareaRecurrente(int idTareaRecurrente, EstadoTarea nuevoEstado, string lenguaje = "es")
        {
            try
            {
                // Obtener la tarea recurrente por Id
                var tareaRecurrente = await _context.RecurrenciaTareas
                    .FirstOrDefaultAsync(r => r.IdRecurrencia == idTareaRecurrente);

                if (tareaRecurrente == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Tarea recurrente no encontrada.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Cambiar el estado de la tarea recurrente
                tareaRecurrente.Estado = nuevoEstado;

                // Guardar los cambios en la base de datos
                _context.RecurrenciaTareas.Update(tareaRecurrente);
                await _context.SaveChangesAsync();

                // Obtener todas las tareas recurrentes asociadas
                var tareasRecurrentes = await _context.RecurrenciaTareas
                    .Where(r => r.IdTareaAsignada == tareaRecurrente.IdTareaAsignada)
                    .ToListAsync();

                // Evaluar el estado general de las tareas recurrentes
                bool todasCompletadas = tareasRecurrentes.All(r => r.Estado == EstadoTarea.Completada);
                bool todasPendientes = tareasRecurrentes.All(r => r.Estado == EstadoTarea.Pendiente);
                bool hayCompletadas = tareasRecurrentes.Any(r => r.Estado == EstadoTarea.Completada);

                // Actualizar el estado de la tarea principal
                var tareaAsignada = await _context.TareaAsignada
                    .FirstOrDefaultAsync(t => t.IdTareaAsignada == tareaRecurrente.IdTareaAsignada);

                if (tareaAsignada != null)
                {
                    if (todasCompletadas)
                    {
                        tareaAsignada.Estado = EstadoTarea.Completada;
                    }
                    else if (todasPendientes)
                    {
                        tareaAsignada.Estado = EstadoTarea.Pendiente;
                    }
                    else if (hayCompletadas)
                    {
                        tareaAsignada.Estado = EstadoTarea.EnCurso;
                    }
                    else
                    {
                        tareaAsignada.Estado = EstadoTarea.Pendiente; // Estado predeterminado si no hay coincidencia
                    }

                    _context.TareaAsignada.Update(tareaAsignada);
                    await _context.SaveChangesAsync();
                }

                var mensaje2 = _firebasetranslate.Traducir("Estado de la tarea recurrente actualizado correctamente.", lenguaje);

                return new OperationResult(true, mensaje2);
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error al actualizar la tarea recurrente", lenguaje);

                // Captura la excepción y devuelve un mensaje de error
                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }

        public async Task<OperationResult> EditarTareaAsignada(int idUsuario, TareaAsignadaDto tareaEditadaDto, string lenguaje = "es")
        {
            // Verificar que el usuario existe
            var usuario = await _context.Usuario
                .Include(u => u.RolFamilia)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            try
            {
                // Buscar la tarea asignada existente con todas las relaciones necesarias
                var tareaAsignada = await _context.TareaAsignada
                    .Include(t => t.TareaDomestica)
                    .Include(t => t.AreaFamilia)
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t => t.IdTareaAsignada == tareaEditadaDto.IdTareaAsignada);

                if (tareaAsignada == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Tarea asignada no encontrada.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Guardar valores originales para la notificación
                var tareaOriginal = new
                {
                    IdUsuario = tareaAsignada.IdUsuario,
                    Estado = tareaAsignada.Estado,
                    FechaInicio = tareaAsignada.FechaInicio,
                    FechaFin = tareaAsignada.FechaFin,
                    DiaSemana = tareaAsignada.DiaSemana,
                    HoraInicio = tareaAsignada.HoraInicio,
                    HoraFin = tareaAsignada.HoraFin
                };

                // Validar permisos del usuario
                if (!usuario.RolFamilia.EsAdmin)
                {
                    // Si no es admin, solo puede actualizar el estado a completado
                    tareaAsignada.Estado = tareaEditadaDto.Estado;
                    await _context.SaveChangesAsync();

                    // Notificación de cambio de estado
                    string resumenNotificacion = $@"
                    Se ha actualizado el estado de tu tarea:
                    - Tarea: {tareaAsignada.TareaDomestica.Nombre}
                    - Área: {tareaAsignada.AreaFamilia.Nombre}
                    - Nuevo estado: {tareaEditadaDto.Estado}";

                    await _notificationService.SendNotificationAsync(
                        "Estado de tarea actualizado",
                        resumenNotificacion,
                        tareaAsignada.IdUsuario);

                    var mensaje = _firebasetranslate.Traducir("Estado de la tarea actualizado a Completado.", lenguaje);

                    return new OperationResult(true, mensaje);
                }

                // Actualizar todos los campos para administradores
                bool recurrenciaModificada = tareaEditadaDto.EsRecurrente &&
                    (tareaEditadaDto.FechaInicio != tareaAsignada.FechaInicio ||
                     tareaEditadaDto.FechaFin != tareaAsignada.FechaFin ||
                     tareaEditadaDto.DiaSemana != tareaAsignada.DiaSemana);

                bool noExistenRecurrencias = tareaEditadaDto.EsRecurrente &&
                    !(await _context.RecurrenciaTareas.AnyAsync(r => r.IdTareaAsignada == tareaEditadaDto.IdTareaAsignada));

                // Actualizar los campos de la tarea
                tareaAsignada.IdUsuario = tareaEditadaDto.IdUsuario;
                tareaAsignada.IdTareaDomestica = tareaEditadaDto.IdTareaDomestica;
                tareaAsignada.IdAreaFamilia = tareaEditadaDto.IdAreaFamilia;
                tareaAsignada.Descripcion = tareaEditadaDto.Descripcion;
                tareaAsignada.FechaInicio = tareaEditadaDto.FechaInicio;
                tareaAsignada.FechaFin = tareaEditadaDto.FechaFin;
                tareaAsignada.Prioridad = tareaEditadaDto.Prioridad;
                tareaAsignada.Estado = tareaEditadaDto.Estado;
                tareaAsignada.EsRecurrente = tareaEditadaDto.EsRecurrente;
                tareaAsignada.DiaSemana = tareaEditadaDto.EsRecurrente ? tareaEditadaDto.DiaSemana : null;
                tareaAsignada.HoraInicio = tareaEditadaDto.HoraInicio;
                tareaAsignada.HoraFin = tareaEditadaDto.HoraFin;

                // Manejar recurrencias
                if (recurrenciaModificada || noExistenRecurrencias)
                {
                    var tareasRecurrentesExistentes = await _context.RecurrenciaTareas
                        .Where(r => r.IdTareaAsignada == tareaEditadaDto.IdTareaAsignada)
                        .ToListAsync();

                    _context.RecurrenciaTareas.RemoveRange(tareasRecurrentesExistentes);

                    DateTime fechaActual = tareaEditadaDto.FechaInicio.Date;
                    int totalRecurrencias = 0;

                    while (fechaActual.DayOfWeek != (DayOfWeek)(((int)tareaEditadaDto.DiaSemana % 7)))
                    {
                        fechaActual = fechaActual.AddDays(1);
                    }

                    while (fechaActual <= tareaEditadaDto.FechaFin.Date)
                    {
                        var tareaRecurrente = new RecurrenciaTareas
                        {
                            IdTareaAsignada = tareaEditadaDto.IdTareaAsignada,
                            Estado = tareaEditadaDto.Estado,
                            FechaDia = fechaActual.Date,
                        };

                        _context.RecurrenciaTareas.Add(tareaRecurrente);
                        fechaActual = fechaActual.AddDays(7);
                        totalRecurrencias++;
                    }

                    // Preparar notificación para tarea recurrente modificada
                    string nombreDiaSemana = ((DiaSemana)tareaEditadaDto.DiaSemana).ToString();
                    string resumenNotificacion = $@"
                    Tu tarea recurrente ha sido modificada:
                    - Tarea: {tareaAsignada.TareaDomestica.Nombre}
                    - Área: {tareaAsignada.AreaFamilia.Nombre}
                    - Nueva programación: Cada {nombreDiaSemana}
                    - Nuevo período: {tareaEditadaDto.FechaInicio:dd/MM/yyyy} - {tareaEditadaDto.FechaFin:dd/MM/yyyy}
                    - Nuevo horario: {tareaEditadaDto.HoraInicio:HH:mm} - {tareaEditadaDto.HoraFin:HH:mm}
                    - Total de ocurrencias: {totalRecurrencias}

                    Modificado por el administrador {usuario.Nombre}";

                    await _notificationService.SendNotificationAsync(
                        "Tarea recurrente modificada",
                        resumenNotificacion,
                        tareaEditadaDto.IdUsuario);
                }
                else if (!tareaEditadaDto.EsRecurrente)
                {
                    var tareasRecurrentesExistentes = await _context.RecurrenciaTareas
                        .Where(r => r.IdTareaAsignada == tareaEditadaDto.IdTareaAsignada)
                        .ToListAsync();

                    _context.RecurrenciaTareas.RemoveRange(tareasRecurrentesExistentes);

                    // Notificación para tarea no recurrente
                    string resumenNotificacion = $@"
                    Tu tarea ha sido modificada:
                    - Tarea: {tareaAsignada.TareaDomestica.Nombre}
                    - Área: {tareaAsignada.AreaFamilia.Nombre}
                    - Nueva fecha: {tareaEditadaDto.FechaInicio:dd/MM/yyyy}
                    - Nuevo horario: {tareaEditadaDto.HoraInicio:HH:mm} - {tareaEditadaDto.HoraFin:HH:mm}
                    - Nuevo estado: {tareaEditadaDto.Estado}

                    Modificado por el administrador {usuario.Nombre}";

                    await _notificationService.SendNotificationAsync(
                        "Tarea modificada",
                        resumenNotificacion,
                        tareaEditadaDto.IdUsuario);
                }

                await _context.SaveChangesAsync();
                var mensaje2 = _firebasetranslate.Traducir("Tarea asignada editada exitosamente.", lenguaje);

                return new OperationResult(true, mensaje2);
            }
            catch (DbUpdateException ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error al editar la tarea asignada", lenguaje);

                return new OperationResult(false, $"{mensaje}: {ex.InnerException?.Message}");
            }
        }

        public async Task<OperationResult> ObtenerTareasPorFamilia(int usuarioId, int familiaId, DateTime fechaInicio, DateTime fechaFin, string lenguaje = "es")
        {
            try
            {

                var usuario = await _context.Usuario.Include(u => u.RolFamilia)
                               .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }
                 
                var rolFamilia = usuario.RolFamilia?.EsAdmin;
                if (!rolFamilia.Value)
                {
                    var tareasAsignadasUsuario = await _context.TareaAsignada
                    .Where(t => t.IdUsuario == usuarioId )
                    .Select(tarea => new  
                    {
                        Dia = tarea.FechaInicio.Date,
                        IdTareaAsignada = tarea.IdTareaAsignada,
                        IdArea = tarea.IdAreaFamilia,
                        AreaNombre = _firebasetranslate.Traducir(tarea.AreaFamilia.Nombre, lenguaje),
                        Area = tarea.AreaFamilia,
                        TareaNombre = _firebasetranslate.Traducir(tarea.TareaDomestica.Nombre, lenguaje),
                        Tarea = tarea.TareaDomestica,
                        //Descripcion = tarea.Descripcion,
                        Descripcion = _firebasetranslate.Traducir(tarea.Descripcion, lenguaje),
                        TipoTareaNombre = _firebasetranslate.Traducir(tarea.TareaDomestica.TipoTarea.Nombre, lenguaje),
                        TipoTarea = tarea.TareaDomestica.TipoTarea,

                        Persona = new { tarea.IdUsuario, tarea.Usuario.Nombre, tarea.Usuario.FotoUrl, tarea.Usuario.IdRolFamilia },
                        FechaInicio = tarea.FechaInicio,
                        FechaFin = tarea.FechaFin,
                        Estado = tarea.Estado,
                        Prioridad = tarea.Prioridad,
                        EsContrato = false,
                        EsRecurrente = tarea.EsRecurrente,
                        DiaSemana = tarea.DiaSemana,
                        HoraFin = tarea.HoraFin,
                        HoraInicio = tarea.HoraInicio,
                        RecurrenciaTareas = tarea.EsRecurrente
                                ? _context.RecurrenciaTareas
                                    .Where(r => r.IdTareaAsignada == tarea.IdTareaAsignada).Select(r => new RecurrenciaTareas
                                    {
                                        IdRecurrencia = r.IdRecurrencia,
                                        IdTareaAsignada = r.IdTareaAsignada,
                                        Estado = r.Estado,
                                        FechaDia = r.FechaDia,
                                    }
                                    ).ToList()
                                : null
                    })
                    .ToListAsync();

                    var mensaje2 = _firebasetranslate.Traducir("Tareas asignadas obtenidas exitosamente.", lenguaje);

                    return new OperationResult(true, mensaje2, tareasAsignadasUsuario);
                }
                else
                {
                    // Obtener las tareas asignadas para la familia, filtrando por IdAreaFamilia en la tabla AreaDelHogar_Familia y el rango de fechas 
                    var tareasAsignadas = await _context.TareaAsignada
                        .Where(t => _context.AreaDelHogar_Familia
                            .Any(a => a.IdAreaFamilia == t.IdAreaFamilia && a.IdFamilia == familiaId) )
                        .Select(tarea => new
                        {
                            Dia = tarea.FechaInicio.Date,
                            IdTareaAsignada = tarea.IdTareaAsignada,
                            IdArea = tarea.IdAreaFamilia,
                            AreaNombre = _firebasetranslate.Traducir(tarea.AreaFamilia.Nombre, lenguaje),

                            Area = tarea.AreaFamilia,
                            TareaNombre = _firebasetranslate.Traducir(tarea.TareaDomestica.Nombre, lenguaje),

                            Tarea = tarea.TareaDomestica,
                            Descripcion = _firebasetranslate.Traducir(tarea.Descripcion, lenguaje),
                            TipoTareaNombre = _firebasetranslate.Traducir(tarea.TareaDomestica.TipoTarea.Nombre, lenguaje),

                            TipoTarea = tarea.TareaDomestica.TipoTarea,
                            Persona = new { tarea.IdUsuario, tarea.Usuario.Nombre, tarea.Usuario.FotoUrl, tarea.Usuario.IdRolFamilia },
                            FechaInicio = tarea.FechaInicio,
                            FechaFin = tarea.FechaFin,
                            Estado = tarea.Estado,
                            Prioridad = tarea.Prioridad,
                            EsContrato = false,
                            EsRecurrente = tarea.EsRecurrente,
                            DiaSemana = tarea.DiaSemana,
                            HoraFin = tarea.HoraFin,
                            HoraInicio = tarea.HoraInicio,
                            RecurrenciaTareas = tarea.EsRecurrente
                                ? _context.RecurrenciaTareas
                                    .Where(r => r.IdTareaAsignada == tarea.IdTareaAsignada).Select(r => new RecurrenciaTareas
                                    {
                                        IdRecurrencia = r.IdRecurrencia,
                                        IdTareaAsignada = r.IdTareaAsignada,
                                        Estado = r.Estado,
                                        FechaDia = r.FechaDia,
                                    }
                                    ).ToList()
                                : null
                        })
                        .ToListAsync();

                    if (tareasAsignadas == null || !tareasAsignadas.Any())
                    {
                        var mensaje = _firebasetranslate.Traducir("No se encontraron tareas asignadas para la familia en el rango de fechas especificado.", lenguaje);

                        return new OperationResult(false, mensaje);
                    }

                    var mensaje2 = _firebasetranslate.Traducir("Tareas asignadas obtenidas exitosamente.", lenguaje);

                    return new OperationResult(true, mensaje2, tareasAsignadas);
                }
                
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error al obtener las tareas asignadas", lenguaje);

                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }
        public async Task<OperationResult> ObtenerTareasYContratosPorFamilia(int familiaId, DateTime fechaInicio, DateTime fechaFin, string lenguaje = "es")
        {
            // Obtener las tareas asignadas para la familia, filtrando por IdAreaFamilia en la tabla AreaDelHogar_Familia y el rango de fechas
            // Obtener las tareas asignadas y sus recurrencias dentro del rango completo
            var tareasAsignadas = await _context.TareaAsignada
                .Where(t => _context.AreaDelHogar_Familia
                    .Any(a => a.IdAreaFamilia == t.IdAreaFamilia && a.IdFamilia == familiaId))
                .Select(tarea => new TareaContratoCalendarioDto
                {
                    Dia = tarea.FechaInicio.Date,
                    IdTareaAsignada = tarea.IdTareaAsignada,
                    IdTareaDomestica = tarea.IdTareaDomestica,
                    IdArea = tarea.IdAreaFamilia,
                    //Area = tarea.AreaFamilia.Nombre,
                    Area = _firebasetranslate.Traducir(tarea.AreaFamilia.Nombre, lenguaje),
                    //Tarea = tarea.TareaDomestica.Nombre,
                    Tarea = _firebasetranslate.Traducir(tarea.TareaDomestica.Nombre, lenguaje),
                    Descripcion = tarea.Descripcion,
                    //Categoria = tarea.TareaDomestica.TipoTarea.Nombre,
                    Categoria = _firebasetranslate.Traducir(tarea.TareaDomestica.TipoTarea.Nombre, lenguaje),
                    IdUsuario = tarea.IdUsuario,
                    Persona = tarea.Usuario.Nombre,
                    FotoUrl = tarea.Usuario.FotoUrl,
                    FechaInicio = tarea.FechaInicio,
                    FechaFin = tarea.FechaFin,
                    Estado = tarea.Estado,
                    HoraInicio = tarea.HoraInicio,
                    HoraFin = tarea.HoraFin,
                    Prioridad = tarea.Prioridad,
                    EsContrato = false,
                    EsRecurrente = tarea.EsRecurrente,
                    DiaSemana = tarea.DiaSemana,
                    RecurrenciaTareas = _context.RecurrenciaTareas
                        .Where(r => r.IdTareaAsignada == tarea.IdTareaAsignada)
                        .Select(r => new RecurrenciaTareas
                        {
                            IdRecurrencia = r.IdRecurrencia,
                            IdTareaAsignada = r.IdTareaAsignada,
                            Estado = r.Estado,
                            FechaDia = r.FechaDia,
                        })
                        .ToList()
                })
                .ToListAsync();

            // Filtrar las tareas y recurrencias dentro del rango solicitado
            var tareasFiltradas = tareasAsignadas
                .Where(t => t.FechaInicio.Date >= fechaInicio.Date && t.FechaInicio.Date <= fechaFin.Date
                         || t.RecurrenciaTareas.Any(r => r.FechaDia.Date >= fechaInicio.Date && r.FechaDia.Date <= fechaFin.Date))
                .ToList();

            var tareasRecurrentes = tareasFiltradas
                .SelectMany(tarea => tarea.RecurrenciaTareas, (tarea, recurrencia) => new TareaContratoCalendarioDto
                {
                    Dia = recurrencia.FechaDia,
                    IdTareaAsignada = tarea.IdTareaAsignada,
                    IdTareaDomestica = tarea.IdTareaDomestica,
                    IdArea = tarea.IdArea,
                    Area = tarea.Area,
                    Tarea = tarea.Tarea,
                    Descripcion = tarea.Descripcion,
                    Categoria = tarea.Categoria,
                    IdUsuario = tarea.IdUsuario,
                    Persona = tarea.Persona,
                    FotoUrl = tarea.FotoUrl,
                    FechaInicio = tarea.FechaInicio,
                    FechaFin = tarea.FechaFin,
                    HoraInicio = tarea.HoraInicio,
                    HoraFin = tarea.HoraFin,
                    Estado = recurrencia.Estado,
                    Prioridad = tarea.Prioridad,
                    EsContrato = tarea.EsContrato,
                    EsRecurrente = tarea.EsRecurrente,
                    DiaSemana = tarea.DiaSemana
                })
                .ToList();




            // Obtener los contratos relacionados con la familia en el rango de fechas
            var contratos = await _context.ContratoPersonal
                .Where(c => c.FechaInicio.Date >= fechaInicio.Date && c.FechaInicio.Date <= fechaFin.Date &&
                            c.IdFamilia == familiaId).Where(c=> c.Estado == EstadoContrato.Solicitado || c.Estado == EstadoContrato.Aceptado )
                .Select(contrato => new
                {
                    Dia = contrato.FechaInicio.Date,
                    ContratoPersonal = contrato,
                    CorreoOfertador = _context.Usuario.Where(u => u.IdUsuario == contrato.Servicio.IdUsuario).Select(u => u.Correo).FirstOrDefault(),
                    FotoOfertador = _context.Usuario.Where(u => u.IdUsuario == contrato.Servicio.IdUsuario).Select(u => u.FotoUrl).FirstOrDefault(),
                    NombreOfertador = _context.Usuario.Where(u => u.IdUsuario == contrato.Servicio.IdUsuario).Select(u => u.Nombre).FirstOrDefault(),
                    EsContrato = true,
                    ServicioId = contrato.Servicio.IdServicio,
                    EstadoContrato = contrato.Estado,
                    NombreServicio = _context.Servicio.Where(s => s.IdServicio == contrato.IdServicioContratado).Select(u => u.Nombre).FirstOrDefault(),

                })
                .ToListAsync();

            // Agrupar tareas y contratos por día
            var tareasYContratosAgrupados = tareasAsignadas.Where(t => t.EsRecurrente == false)
                .Select(t => new TareaContratoCalendarioDto
                {
                    Dia = t.Dia,
                    IdTareaAsignada = (int?)t.IdTareaAsignada,
                    IdTareaDomestica = (int?)t.IdTareaDomestica,
                    IdArea = (int?)t.IdArea,
                    Area = t.Area,
                    Tarea = t.Tarea,
                    Descripcion = t.Descripcion,
                    Categoria = t.Categoria,
                    IdUsuario = (int?)t.IdUsuario,
                    Persona = t.Persona,
                    FotoUrl = t.FotoUrl,
                    FechaInicio = t.FechaInicio,
                    FechaFin = t.FechaFin,
                    Estado = t.Estado,
                    Prioridad = t.Prioridad,
                    EsContrato = t.EsContrato,
                    EsRecurrente = t.EsRecurrente,
                    DiaSemana = t.DiaSemana,

                    HoraInicio = t.HoraInicio,
                    HoraFin = t.HoraFin,
                })
                .Concat(contratos.Select(c => new TareaContratoCalendarioDto
                {
                    Dia = c.Dia,
                    IdTareaAsignada = (int?)null,
                    IdTareaDomestica = (int?)null,
                    IdArea = (int?)null,
                    Area = "Contrato",
                    Tarea = $"Contratación de {c.NombreServicio}",
                    Descripcion = $"Contrato con {c.NombreOfertador}",
                    Categoria = "",
                    IdUsuario = c.ContratoPersonal.Servicio != null ? (int?)c.ContratoPersonal.Servicio.IdUsuario : null,
                    Persona = c.NombreOfertador,
                    FotoUrl = c.FotoOfertador,
                    FechaInicio = c.ContratoPersonal.FechaInicio,
                    FechaFin = c.ContratoPersonal.FechaInicio.AddHours(1), // Suponiendo duración de 1 hora
                    Estado = EstadoTarea.Pendiente,
                    EstadoContrato = c.EstadoContrato,
                    Prioridad = "",
                    EsContrato = c.EsContrato, // Ensure that EsContrato is correctly set for contracts
                    EsRecurrente = false,
                    DiaSemana =  0,
                    IdServicio = c.ServicioId
                })
                .Concat(tareasRecurrentes)
                )
                .GroupBy(t => t.Dia)
                .Select(g => new TareasContratosPorDiaDTO
                {
                    Dia = g.Key,
                    ListaAsignaciones = g.Select(t => new TareaContratoCalendarioDto
                    {
                        IdTareaAsignada = t.IdTareaAsignada,
                        IdTareaDomestica = t.IdTareaDomestica,
                        IdArea = t.IdArea,
                        Area = t.Area,
                        Tarea = t.Tarea,
                        Descripcion = t.Descripcion,
                        Categoria = t.Categoria,
                        IdUsuario = t.IdUsuario,
                        Persona = t.Persona,
                        FotoUrl = t.FotoUrl,
                        FechaInicio = t.FechaInicio,
                        FechaFin = t.FechaFin,
                        Estado = t.Estado,
                        Prioridad = t.Prioridad,
                        EsContrato = t.EsContrato,
                        EstadoContrato = t.EstadoContrato,
                        EsRecurrente = t.EsRecurrente,
                        DiaSemana = t.DiaSemana, 
                        HoraInicio = t.HoraInicio,
                        HoraFin = t.HoraFin,
                    }).ToList()
                })
                .ToList();

            var mensaje = _firebasetranslate.Traducir("Tareas y contratos obtenidos exitosamente.", lenguaje);

            return new OperationResult(true, mensaje, tareasYContratosAgrupados);
        }
        public async Task<OperationResult> EliminarTareaAsignada(int idUsuario, int idTareaAsignada, string lenguaje = "es")
        {
            try
            {
                // Verificar que el usuario existe y tiene el rol de familia "Admin"
                var usuario = await _context.Usuario
                    .Include(u => u.RolFamilia)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                if (!usuario.RolFamilia.EsAdmin)
                {
                    var mensaje = _firebasetranslate.Traducir("No tienes permisos para realizar esta acción.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Buscar la tarea asignada por su ID incluyendo todas las relaciones necesarias
                var tareaAsignada = await _context.TareaAsignada
                    .Include(t => t.Recurrencias)
                    .Include(t => t.TareaDomestica)
                    .Include(t => t.AreaFamilia)
                    .Include(t => t.Usuario) // Usuario al que estaba asignada la tarea
                    .FirstOrDefaultAsync(t => t.IdTareaAsignada == idTareaAsignada);

                if (tareaAsignada == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Tarea asignada no encontrada.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Preparar el mensaje de notificación antes de eliminar
                string resumenNotificacion;
                if (tareaAsignada.EsRecurrente)
                {
                    string nombreDiaSemana = ((DiaSemana)tareaAsignada.DiaSemana).ToString();
                    int totalRecurrenciasEliminadas = tareaAsignada.Recurrencias?.Count ?? 0;

                    resumenNotificacion = $@"
                    Se ha eliminado la siguiente tarea recurrente:
                    - Tarea: {tareaAsignada.TareaDomestica.Nombre}
                    - Área: {tareaAsignada.AreaFamilia.Nombre}
                    - Programación: Cada {nombreDiaSemana}
                    - Período: {tareaAsignada.FechaInicio:dd/MM/yyyy} - {tareaAsignada.FechaFin:dd/MM/yyyy}
                    - Total de ocurrencias eliminadas: {totalRecurrenciasEliminadas}
                    - Horario: {tareaAsignada.HoraInicio:HH:mm} - {tareaAsignada.HoraFin:HH:mm}

                    Esta tarea ha sido eliminada por el administrador {usuario.Nombre}.";
                }
                else
                {
                    resumenNotificacion = $@"
                    Se ha eliminado la siguiente tarea:
                    - Tarea: {tareaAsignada.TareaDomestica.Nombre}
                    - Área: {tareaAsignada.AreaFamilia.Nombre}
                    - Fecha: {tareaAsignada.FechaInicio:dd/MM/yyyy}
                    - Horario: {tareaAsignada.HoraInicio:HH:mm} - {tareaAsignada.HoraFin:HH:mm}

                    Esta tarea ha sido eliminada por el administrador {usuario.Nombre}.";
                }

                // Eliminar las recurrencias asociadas si existen
                if (tareaAsignada.Recurrencias != null && tareaAsignada.Recurrencias.Any())
                {
                    _context.RecurrenciaTareas.RemoveRange(tareaAsignada.Recurrencias);
                }

                // Eliminar la tarea asignada
                _context.TareaAsignada.Remove(tareaAsignada);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                // Enviar notificación al usuario que tenía asignada la tarea
                await _notificationService.SendNotificationAsync(
                    "Tarea eliminada",
                    resumenNotificacion,
                    tareaAsignada.Usuario.IdUsuario);

                var mensaje2 = _firebasetranslate.Traducir("Tarea asignada eliminada exitosamente.", lenguaje);


                return new OperationResult(true, mensaje2);
            }
            catch (DbUpdateException ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error al eliminar la tarea asignada", lenguaje);

                return new OperationResult(false, $"{mensaje}: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error inesperado", lenguaje);

                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }

    }
}
