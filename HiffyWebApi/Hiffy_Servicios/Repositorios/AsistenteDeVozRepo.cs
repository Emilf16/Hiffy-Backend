using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Contracts;
using static Hiffy_Entidades.Entidades.TareaAsignada;

namespace Hiffy_Servicios.Repositorios
{
    public class AsistenteDeVozRepo  : IAsistenteDeVoz
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AreaDelHogarRepo _areaDelHogarRepo;
        private readonly TareaDomesticaRepo _tareaDomesticaRepositorio;
        private readonly TareaAsignadaRepo _tareaAsignadaRepo;

        public AsistenteDeVozRepo (AppDbContext context, IConfiguration configuration, AreaDelHogarRepo areaDelHogarRepo, TareaDomesticaRepo tareaDomesticaRepositorio)
        {
            _context = context;
            _configuration = configuration;
            _areaDelHogarRepo = areaDelHogarRepo;
            _tareaDomesticaRepositorio = tareaDomesticaRepositorio;
        }
        public async Task<bool> ExisteDispositivoFamilia(string nombre, string idDispositivo, string lenguaje = "es")
        {
            bool existe = await _context.DispositivoFamilia
                .AsNoTracking()
                .Where(x => x.NombreDispositivo.Trim().ToUpper() == nombre.Trim().ToUpper()
                    && x.IdDispositivo.Trim().ToUpper() != idDispositivo.Trim().ToUpper())
                .AnyAsync();

            return existe;
        }
        public async Task<OperationResult> ValidarDispositivo(string idDispositivo, string lenguaje = "es")
        {

            var dispositivo = await _context.DispositivoFamilia
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdDispositivo.ToLower() == idDispositivo.ToLower());

            if (dispositivo == null)
            {
                return new OperationResult(false, "El dispositivo no está registrado a ninguna familia.");
            }

            // Verificar si el área pertenece a la familia
            var areaAsignada = await _context.AreaDelHogar_Familia
                .FirstOrDefaultAsync(a => a.IdAreaFamilia == dispositivo.IdAreaFamilia);

            if (areaAsignada == null)
            {
                return new OperationResult(false, "El dispositivo no está asignado a ninguna área disponible.", 0);
            }

            // Obtener la familia asociada
            var familia = await _context.Familia
                .FirstOrDefaultAsync(x => x.IdFamilia == areaAsignada.IdFamilia);

            if (familia == null)
            {
                return new OperationResult(false, "No se encontró la familia asociada.");
            }


            if (dispositivo.Estado != EstadoDispositivo.Aceptado)
            {
                return new OperationResult(false, "El dispositivo no está activado para esta familia. Por favor, valide el estado del dispositivo en la app.");
            }

            return new OperationResult(true, "Dispositivo validado exitosamente.");
        }
        //METODOS FRONTEND
        public async Task<OperationResult> PutActualizarDatosDispositivo(PutDispositivoFamilia dispositivo, int usuarioId, string lenguaje = "es")
        {
            try
            {

            bool existe = await ExisteDispositivoFamilia(dispositivo.NombreDispositivo, dispositivo.IdDispositivo);
            if (!existe)
            {
                var dispositivoInteligente = await _context.DispositivoFamilia.Where(x => x.IdDispositivo == dispositivo.IdDispositivo).FirstOrDefaultAsync();

                if (dispositivoInteligente == null)
                {
                    return new OperationResult(false, "El dispositivo no fue encontrado.", dispositivo.IdDispositivo);
                }

                dispositivoInteligente.NombreDispositivo = dispositivo.NombreDispositivo;
                dispositivoInteligente.IdAreaFamilia = dispositivo.IdAreaFamilia;
                dispositivoInteligente.Estado = dispositivo.Estado;
                await _context.SaveChangesAsync();

                return new OperationResult(true, "Actualización Exitosa.", dispositivoInteligente.IdDispositivo);
            }
            else
            {
                return new OperationResult(false, "Ya existe un registro con este nombre.", 0);
            }
            }catch(Exception ex)
            {
                return new OperationResult(false, "Error al actualizar su dispositivo.", 0);

            }
        }
        public async Task<OperationResult> DeleteDispositivoFamilia(string idDispositivo, int usuarioId, string lenguaje = "es")
        {
            var dispositivo = _context.DispositivoFamilia.FirstOrDefault(a => a.IdDispositivo == idDispositivo);
            if (dispositivo == null)
            {
                return new OperationResult(false, "Asignación no encontrada para el código de dispositivo especificado.");
            }

            // Eliminar la dispositivo
            _context.DispositivoFamilia.Remove(dispositivo);
            await _context.SaveChangesAsync();

            return new OperationResult(true, "Dispositivo eliminado exitosamente");
        }
        public async Task<OperationResult> GetMisDispositivos(int idFamilia, string lenguaje = "es")
        {
            try
            {
                // Obtener las áreas asignadas a la familia
                var areas = await _context.AreaDelHogar_Familia
                    .AsNoTracking()
                    .Where(a => a.IdFamilia == idFamilia)
                    .Select(a => a.IdAreaFamilia)
                    .ToListAsync();

                if (areas == null || !areas.Any())
                {
                    return new OperationResult(false, "No se encontraron áreas asociadas a la familia especificada.");
                }

                // Obtener los dispositivos en las áreas relacionadas
                var dispositivos = await _context.DispositivoFamilia
                    .AsNoTracking()
                    .Where(d => areas.Contains(d.IdAreaFamilia))
                    .ToListAsync();

                if (dispositivos == null || !dispositivos.Any())
                {
                    return new OperationResult(false, "No se encontraron dispositivos en las áreas asociadas a la familia especificada.");
                }

                return new OperationResult(true, "Listado de dispositivos obtenido exitosamente.", dispositivos);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, "Ocurrió un error al obtener los dispositivos: " + ex.Message);
            }
        }


        //METODOS DISPOSITIVO INTELIGENTE 
        public async Task<OperationResult> GetInformacionFamiliar(string idDispositivo, string lenguaje = "es")
        {
            if (string.IsNullOrEmpty(idDispositivo))
            {
                return new OperationResult(false, "Se necesita el código del dispositivo.", 0);
            }

            var dispositivo = await _context.DispositivoFamilia
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdDispositivo.ToLower() == idDispositivo.ToLower());

            if (dispositivo == null)
            {
                return new OperationResult(false, "El dispositivo no está registrado a ninguna familia.", 0);
            }

            // Verificar si el área pertenece a la familia
            var areaAsignada = await _context.AreaDelHogar_Familia
                .FirstOrDefaultAsync(a => a.IdAreaFamilia == dispositivo.IdAreaFamilia);

            if (areaAsignada == null)
            {
                return new OperationResult(false, "El dispositivo no está asignado a ninguna área disponible.", 0);
            }

            // Obtener la familia asociada
            var familia = await _context.Familia
                .FirstOrDefaultAsync(x => x.IdFamilia == areaAsignada.IdFamilia);

            if (familia == null)
            {
                return new OperationResult(false, "No encontré una familia asociada a este dispositivo. Necesito registrar este dispositivo con tu familia.", 0);
            }

            if (dispositivo.Estado != EstadoDispositivo.Aceptado)
            {
                return new OperationResult(false, "El dispositivo no está activado para esta familia. Por favor, valide el estado del dispositivo en la app.", 1);
            }

            // Obtener el estado activo para miembros familiares
            var estadoMiembroFamiliaActivo = await _context.EstadoFamilia
                .Where(x => x.Activo)
                .Select(x => x.IdEstadoFamilia)
                .FirstOrDefaultAsync();

            if (estadoMiembroFamiliaActivo == 0)
            {
                return new OperationResult(false, "No se encontró el estado activo de miembros familiares.");
            }

            // Consultar miembros familiares
            var miembrosFamiliares = await _context.Usuario
                .Where(x => x.IdFamilia == familia.IdFamilia && x.IdEstadoFamilia == estadoMiembroFamiliaActivo)
                .Select(x => new ItemDto
                {
                    Id = x.IdUsuario,
                    Name = x.Nombre
                })
                .ToListAsync();

            // Consultar áreas activas de la familia
            var areasActivasFamilia = await _context.AreaDelHogar_Familia
                .Where(x => x.IdFamilia == familia.IdFamilia && x.IdEstadoAreasDelHogar == EstadoAreasDelHogar.Activo)
                .Select(x => new ItemDto
                {
                    Id = x.IdAreaFamilia,
                    Name = x.Nombre
                })
                .ToListAsync();

            // Consultar tareas domésticas activas
            var tareasDomesticasActivasFamilia = await _tareaDomesticaRepositorio.MostrarTareasDomesticas(true, familia.IdFamilia, true);
            // Filtrar las tareas activas de la respuesta
            List<ItemDto> tareasActivas = new List<ItemDto>();

            if (tareasDomesticasActivasFamilia?.Data is List<TareaDomesticaGetDto> tareasDomesticasList)
            {
                tareasActivas = tareasDomesticasList
                    .Where(t => t.IdEstadoTarea == EstadoTareaDomestica.Activo)
                    .Select(t => new ItemDto
                    {
                        Id = t.IdTareaDomestica,
                        Name = t.Nombre
                    })
                    .ToList();
            } 
            // Crear el objeto de respuesta
            var response = new FamilyInfoResponse
            {
                FamilyMembers = miembrosFamiliares,
                HouseAreas = areasActivasFamilia,
                Tasks = tareasActivas
            };

            return new OperationResult(true, "Información familiar obtenida exitosamente.", response);
        }
        public async Task<OperationResult> GetAreasDelHogarFamiliar(string codigoFamilia, int edad, string lenguaje = "es")
        {


            // Validate input parameters
            if (string.IsNullOrEmpty(codigoFamilia) || codigoFamilia.Length != 8 || !codigoFamilia.All(char.IsDigit))
            {
                return new OperationResult(false, "El código de familia debe ser un número de 8 dígitos.");
            }

            if (edad <= 0)
            {
                return new OperationResult(false, "La edad del usuario mayor debe ser un número positivo.");
            }

            // Buscar la familia con el código proporcionado
            var familia = await _context.Familia.FirstOrDefaultAsync(f => f.CodigoFamilia == codigoFamilia.ToString());
            if (familia == null)
            {
                return new OperationResult(false, "Familia no encontrada");
            }

            // Verificar si existe un usuario en la familia con la edad proporcionada
            var usuarios = await _context.Usuario
            .Where(u => u.IdFamilia == familia.IdFamilia)
            .ToListAsync();

            var usuario = usuarios.FirstOrDefault(u => CalcularEdad(u.FechaNacimiento) == edad);

            if (usuario == null)
            {
                return new OperationResult(false, "Usuario no encontrado o la edad no coincide");
            }

           
            // Verificar si el usuario tiene un rol de administrador en la familia
            var rolFamilia = await _context.RolFamilia
                .FirstOrDefaultAsync(r => r.IdRolFamilia == usuario.IdRolFamilia && r.EsAdmin);

            if (rolFamilia == null)
            {
                return new OperationResult(false, "El usuario no tiene permisos de administrador en la familia");
            }

            // Obtener las áreas del hogar
            var areas = await _areaDelHogarRepo.MostrarAreasDelHogar(false, familia.IdFamilia, true);

            return areas;
        }

        // Método auxiliar para calcular la edad a partir de la fecha de nacimiento
        private int CalcularEdad(DateTime fechaNacimiento, string lenguaje = "es")
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;

            // Ajustar si la fecha de nacimiento aún no ha ocurrido este año
            if (fechaNacimiento.Date > hoy.AddYears(-edad))
            {
                edad--;
            }

            return edad;
        }
        public async Task<OperationResult> PostRegistrarDispositivo(PostDispositivoDto dto, int codigoFamilia, string lenguaje = "es")
        {
            try
            {
                // Verificar si existe la familia con el código proporcionado
                var familia = await _context.Familia.FirstOrDefaultAsync(f => f.CodigoFamilia == codigoFamilia.ToString());
                if (familia == null)
                {
                    return new OperationResult(false, "La familia con el código proporcionado no existe.", 0);
                }

                // Verificar si el área pertenece a la familia
                var areaAsignada = await _context.AreaDelHogar_Familia.FirstOrDefaultAsync(a => a.IdAreaFamilia == dto.IdAreaFamilia && a.IdFamilia == familia.IdFamilia);
                if (areaAsignada == null)
                {
                    return new OperationResult(false, "El área especificada no está asignada a la familia.", 0);
                }

                // Crear y registrar el dispositivo
                var dispositivoFamilia = new DispositivoFamilia
                {
                    NombreDispositivo = "Nuevo Asistente de Voz",
                    IdAreaFamilia = dto.IdAreaFamilia,
                    IdDispositivo = dto.IdDispositivo,
                    Estado = EstadoDispositivo.Solicitado,
                };

                await _context.DispositivoFamilia.AddAsync(dispositivoFamilia);
                await _context.SaveChangesAsync();

                return new OperationResult(true, "Dispositivo registrado exitosamente.", dispositivoFamilia.IdDispositivoFamilia);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, "Error al intentar registrar su dispositivo, favor intente más tarde.", 0);
            }
        }

        public async Task<OperationResult> DeleteAllAsignaciones(string idDispositivo, string lenguaje = "es")
        {
            throw new NotImplementedException();
        }
        public async Task<OperationResult> DeleteAsignacionPorId(string idDispositivo, int idAsignacion, string lenguaje = "es")
        {
            try
            {

                // Verificar que el dispositivo este activo
                var validacionDispositivo = await ValidarDispositivo(idDispositivo);
                if (!validacionDispositivo.Success)
                {
                    return validacionDispositivo; // Devuelve el mensaje de error si la validación falla
                }
                // Verificar si la asignación existe
                var asignacionToDelete = await _context.TareaAsignada
                .FirstOrDefaultAsync(a => a.IdTareaAsignada == idAsignacion);

            if (asignacionToDelete == null)
            {
                return new OperationResult(false, "Asignación no encontrada para el código de dispositivo especificado.");
            }

            // Verificar si hay recurrencias asociadas a la asignación
            var recurrenciasToDelete = await _context.RecurrenciaTareas
                .Where(r => r.IdTareaAsignada == idAsignacion)
                .ToListAsync();

            // Eliminar las recurrencias asociadas
            if (recurrenciasToDelete.Any())
            {
                _context.RecurrenciaTareas.RemoveRange(recurrenciasToDelete);
            }

            // Eliminar la asignación
            _context.TareaAsignada.Remove(asignacionToDelete);
            await _context.SaveChangesAsync();

            return new OperationResult(true, "Asignación y recurrencias asociadas eliminadas exitosamente.");

            }
            catch (Exception ex) {
                return new OperationResult(false, $"Error al eliminar la tarea asignada");
            }
        }
        public async Task<OperationResult> GetAsignacionesPorFecha(string idDispositivo, DateTime fechaConsulta, string lenguaje = "es")
        {
            if (string.IsNullOrEmpty(idDispositivo))
            {
                return new OperationResult(false, "Se necesita el código del dispositivo.");
            }

            var dispositivo = await _context.DispositivoFamilia
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdDispositivo.ToLower() == idDispositivo.ToLower());

            if (dispositivo == null)
            {
                return new OperationResult(false, "El dispositivo no está registrado a ninguna familia.");
            }

            // Verificar si el área pertenece a la familia
            var areaAsignada = await _context.AreaDelHogar_Familia
                .FirstOrDefaultAsync(a => a.IdAreaFamilia == dispositivo.IdAreaFamilia);

            if (areaAsignada == null)
            {
                return new OperationResult(false, "El dispositivo no está asignado a ninguna área disponible.", 0);
            }

            // Obtener la familia asociada
            var familia = await _context.Familia
                .FirstOrDefaultAsync(x => x.IdFamilia == areaAsignada.IdFamilia);

            if (familia == null)
            {
                return new OperationResult(false, "No se encontró la familia asociada.");
            }



            var fechaInicioMes = new DateTime(fechaConsulta.Year, fechaConsulta.Month, 1);
            var fechaFinMes = fechaInicioMes.AddMonths(1).AddDays(-1);
            // Obtener las tareas asignadas para la familia, filtrando por IdAreaFamilia en la tabla AreaDelHogar_Familia y el rango de fechas
            var tareasAsignadas = await _context.TareaAsignada
                .Where(t => _context.AreaDelHogar_Familia
                    .Any(a => a.IdAreaFamilia == t.IdAreaFamilia && a.IdFamilia == familia.IdFamilia) &&
                    t.FechaInicio.Date >= fechaInicioMes.Date && t.FechaInicio.Date <= fechaFinMes.Date)
                .Select(tarea => new TareaContratoCalendarioDto
                {
                    Dia = tarea.FechaInicio.Date,
                    IdTareaAsignada = tarea.IdTareaAsignada,
                    IdTareaDomestica = tarea.IdTareaDomestica,
                    IdArea = tarea.IdAreaFamilia,
                    Area = tarea.AreaFamilia.Nombre,
                    Tarea = tarea.TareaDomestica.Nombre,
                    Descripcion = tarea.Descripcion,
                    Categoria = tarea.TareaDomestica.TipoTarea.Nombre,
                    IdUsuario = tarea.IdUsuario,
                    Persona = tarea.Usuario.Nombre,
                    FotoUrl = tarea.Usuario.FotoUrl,
                    FechaInicio = tarea.FechaInicio,
                    FechaFin = tarea.FechaFin,
                    Estado = tarea.Estado,
                    Prioridad = tarea.Prioridad,
                    EsContrato = false,
                    EsRecurrente = tarea.EsRecurrente,
                    DiaSemana = tarea.DiaSemana,
                    HoraInicio = tarea.HoraInicio,
                    HoraFin = tarea.HoraFin,
                    RecurrenciaTareas = _context.RecurrenciaTareas.Where(r => r.IdTareaAsignada == tarea.IdTareaAsignada).Select(r => new RecurrenciaTareas
                    {
                        IdRecurrencia = r.IdRecurrencia,
                        IdTareaAsignada = r.IdTareaAsignada,
                        Estado = r.Estado,
                        FechaDia = r.FechaDia,
                    }).ToList()

                })
                .ToListAsync();

            var tareasRecurrentes = tareasAsignadas
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
                 Estado = recurrencia.Estado,
                 Prioridad = tarea.Prioridad,
                 EsContrato = tarea.EsContrato,
                 EsRecurrente = tarea.EsRecurrente,
                 DiaSemana = tarea.DiaSemana,
                   HoraInicio = tarea.HoraInicio,
                 HoraFin = tarea.HoraFin,
             })
             .ToList();
             
            // Agrupar tareas y contratos por día
            var tareasAgrupadas = tareasAsignadas.Where(t => t.EsRecurrente == false && t.Estado == TareaAsignada.EstadoTarea.Pendiente)
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
               
                .Concat(tareasRecurrentes.Where(t =>  t.Estado == TareaAsignada.EstadoTarea.Pendiente))
                .GroupBy(t => t.Dia)
                .Where(g => g.Key == fechaConsulta.Date)
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
                        EsRecurrente = t.EsRecurrente,
                        DiaSemana = t.DiaSemana,
                        HoraInicio = t.HoraInicio,
                        HoraFin = t.HoraFin,
                    }).ToList()
                })
                .ToList();


            return new OperationResult(true, tareasAgrupadas.IsNullOrEmpty()? "No posee tareas disponibles." : "Tareas obtenidas exitosamente." , tareasAgrupadas);
        }
        public async Task<OperationResult> GetContratosPorFecha(string idDispositivo, DateTime fechaConsulta, EstadoContrato estadoContrato, string lenguaje = "es")
        {
            if (string.IsNullOrEmpty(idDispositivo))
            {
                return new OperationResult(false, "Se necesita el código del dispositivo.");
            }

            var dispositivo = await _context.DispositivoFamilia
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdDispositivo.ToLower() == idDispositivo.ToLower());

            if (dispositivo == null)
            {
                return new OperationResult(false, "El dispositivo no está registrado a ninguna familia.");
            }

            // Verificar si el área pertenece a la familia
            var areaAsignada = await _context.AreaDelHogar_Familia
                .FirstOrDefaultAsync(a => a.IdAreaFamilia == dispositivo.IdAreaFamilia);

            if (areaAsignada == null)
            {
                return new OperationResult(false, "El dispositivo no está asignado a ninguna área disponible.", 0);
            }

            // Obtener la familia asociada
            var familia = await _context.Familia
                .FirstOrDefaultAsync(x => x.IdFamilia == areaAsignada.IdFamilia);

            if (familia == null)
            {
                return new OperationResult(false, "No se encontró la familia asociada.");
            }
             

            var fechaInicioMes = new DateTime(fechaConsulta.Year, fechaConsulta.Month, 1);
            var fechaFinMes = fechaInicioMes.AddMonths(1).AddDays(-1);
            var contratos = await _context.ContratoPersonal
             .Where(c => c.FechaInicio.Date >= fechaInicioMes.Date && c.FechaInicio.Date <= fechaFinMes.Date &&
                         c.IdFamilia == familia.IdFamilia && c.Estado == estadoContrato)
             .Select(contrato => new
             {
                 Dia = contrato.FechaInicio.Date,
                 ContratoPersonal = contrato,
                 CorreoOfertador = _context.Usuario.Where(u => u.IdUsuario == contrato.Servicio.IdUsuario).Select(u => u.Correo).FirstOrDefault(),
                 FotoOfertador = _context.Usuario.Where(u => u.IdUsuario == contrato.Servicio.IdUsuario).Select(u => u.FotoUrl).FirstOrDefault(),
                 NombreOfertador = _context.Usuario.Where(u => u.IdUsuario == contrato.Servicio.IdUsuario).Select(u => u.Nombre).FirstOrDefault(),
                 EsContrato = true,
                 NombreServicio = _context.Servicio.Where(s => s.IdServicio == contrato.IdServicioContratado).Select(u => u.Nombre).FirstOrDefault(),
                 ContratoId = contrato.IdContrato
             })
             .ToListAsync();

                // Agrupar tareas y contratos por día
                var contratosAgrupados = contratos.Select(c => new TareaContratoCalendarioDto
                {
                    Dia = c.Dia,
                    IdTareaAsignada = (int?)null,
                    IdTareaDomestica = (int?)null,
                    IdArea = (int?)null,
                    Area = "Contrato",
                    Tarea = "Contrato de Servicio",
                    Descripcion = c.NombreServicio,
                    Categoria = "",
                    IdUsuario = c.ContratoPersonal.Servicio != null ? (int?)c.ContratoPersonal.Servicio.IdUsuario : null,
                    Persona = c.NombreOfertador,
                    FotoUrl = c.FotoOfertador,
                    FechaInicio = c.ContratoPersonal.FechaInicio,
                    FechaFin = c.ContratoPersonal.FechaInicio.AddHours(1), // Suponiendo duración de 1 hora
                    EstadoContrato = c.ContratoPersonal.Estado,
                    Prioridad = "",
                    EsContrato = c.EsContrato, // Ensure that EsContrato is correctly set for contracts
                    EsRecurrente = false,
                    DiaSemana = 0,
                    ContratoId = c.ContratoId
                })
                .GroupBy(t => t.Dia)
                .Where(g => g.Key == fechaConsulta.Date)
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
                        EstadoContrato = t.EstadoContrato,
                        Prioridad = t.Prioridad,
                        EsContrato = t.EsContrato,
                        EsRecurrente = t.EsRecurrente,
                        DiaSemana = t.DiaSemana,
                        HoraInicio = t.HoraInicio,
                        HoraFin = t.HoraFin,
                        ContratoId = t.ContratoId
                    }).ToList()
                })
                .ToList();


            return new OperationResult(true, contratosAgrupados.IsNullOrEmpty() ? "No tiene contrataciones disponibles." : "Contrataciones obtenidas exitosamente.", contratosAgrupados);
        }
        public async Task<OperationResult> PostAsignacion(string idDispositivo, TareaAsignadaDto tareaAsignadaDto, string lenguaje = "es")
        {
            try
            {
                // Verificar que el usuario existe
                var dispositivo = _context.DispositivoFamilia.FirstOrDefault(a => a.IdDispositivo == idDispositivo);
                if (dispositivo == null)
                {
                    return new OperationResult(false, "Asignación no encontrada para el código de dispositivo especificado.");
                }
                if (dispositivo.Estado != EstadoDispositivo.Aceptado)
                {
                    return new OperationResult(false, "El dispositivo no está activado para esta familia, favor validar el estado del mismo.");
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
                    DiaSemana = tareaAsignadaDto.EsRecurrente ? tareaAsignadaDto.DiaSemana : null,
                    HoraInicio = tareaAsignadaDto.HoraInicio,
                    HoraFin = tareaAsignadaDto.HoraFin,
                };
                // Guardar la tarea asignada en la base de datos
                _context.TareaAsignada.Add(tareaAsignada);
                await _context.SaveChangesAsync();

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

                        // Avanzar 7 días para encontrar el siguiente día de la semana
                        fechaActual = fechaActual.AddDays(7);
                    }
                }
                await _context.SaveChangesAsync();
                return new OperationResult(true, "Tarea asignada creada exitosamente.");
            }

            catch (DbUpdateException ex)
            {
                // Captura la excepción interna y devuelve el mensaje detallado
                return new OperationResult(false, $"Error al crear la tarea asignada: {ex.InnerException?.Message}");
            }

        }
    }
}
