using Google.Cloud.Translation.V2;
using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hiffy_Servicios.Repositorios
{
    public class TareaDomesticaRepo
    {
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;
        public TareaDomesticaRepo(AppDbContext context, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _firebasetranslate = firebasetranslate; 
        }

        public async Task<OperationResult> MostrarTareasDomesticas(bool buscarPredeterminados, int IdFamilia, bool incluirActivas, string lenguaje="es")
        {
            try
            {
                List<TareaDomestica> tareasDomesticas;

                if (buscarPredeterminados)
                {
                    // Obtener todas las tareas predeterminadas
                    tareasDomesticas = await _context.TareaDomestica
                        .Include(t => t.TipoTarea)
                        .Where(t => t.Predeterminado)
                        .ToListAsync();
                }
                else
                {
                    // Obtener la lista de tareas desactivadas por el usuario
                    var tareasDesactivadas = await _context.TareasDesactivadas
                        .Where(td => td.IdFamilia == IdFamilia)
                        .Select(td => td.IdTareaDomestica)
                        .ToListAsync();

                    // Obtener todas las tareas (activas y desactivadas)
                    tareasDomesticas = await _context.TareaDomestica
                        .Include(t => t.TipoTarea)
                        .Where(t => t.IdFamilia == IdFamilia || t.Predeterminado)
                        .ToListAsync();

                    // Marcar las desactivadas con IdEstadoTarea = 2
                    foreach (var tarea in tareasDomesticas)
                    {
                        if (tareasDesactivadas.Contains(tarea.IdTareaDomestica))
                        {
                            tarea.IdEstadoTarea = EstadoTareaDomestica.Desactivada; // Cambiar el estado a "2" para tareas desactivadas
                        }
                    }
                }

                // Si incluirActivas es true, filtrar las tareas activas (IdEstadoTarea == 1)
                if (incluirActivas)
                {
                    tareasDomesticas = tareasDomesticas
                        .Where(t => t.IdEstadoTarea == EstadoTareaDomestica.Activo) // Asume que "1" es el estado activo
                        .ToList();
                }

                // Convertir a DTO
                var tareasDto = tareasDomesticas
                    .Select(tarea => new TareaDomesticaGetDto
                    {
                        IdTareaDomestica = tarea.IdTareaDomestica,
                        IdFamilia = tarea.IdFamilia,
                        Nombre = _firebasetranslate.Traducir(tarea.Nombre, lenguaje),
                        Descripcion = _firebasetranslate.Traducir(tarea.Descripcion, lenguaje) ,
                        IdTipoTarea = tarea.TipoTarea.IdTipoTarea,
                        TipoTarea = tarea.TipoTarea,
                        Predeterminado = tarea.Predeterminado,
                        IdEstadoTarea = tarea.IdEstadoTarea,
                    })
                    .ToList();


                var mensaje = _firebasetranslate.Traducir("Acción exitosa", lenguaje);
                return new OperationResult(true, mensaje, tareasDto);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }




        public async Task<OperationResult> CrearTareaDomestica(int idUsuario, TareaDomesticaPostDto nuevaTarea, string lenguaje = "es")
        {
            // Obtener el IdFamilia asociado al usuario
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                var mensaje1 = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                return new OperationResult(false, mensaje1);
            }

            // Validar que no exista una tarea con el mismo nombre en la familia o como predeterminada
            var tareaExistente = await _context.TareaDomestica
                .FirstOrDefaultAsync(t => t.Nombre == nuevaTarea.Nombre
                && (t.IdFamilia == usuario.IdFamilia || nuevaTarea.Predeterminado));

            if (tareaExistente != null)
            {
                var mensaje2 = _firebasetranslate.Traducir("Ya existe una tarea con ese nombre en la familia o como predeterminada.", lenguaje);
                return new OperationResult(false, mensaje2);
            }

            // Crear una nueva tarea doméstica asociada a la familia del usuario con el estado especificado
            var tareaDomestica = new TareaDomestica
            {
                IdFamilia = usuario.IdFamilia,
                Nombre = nuevaTarea.Nombre,
                Descripcion = nuevaTarea.Descripcion,
                IdTipoTarea = nuevaTarea.IdTipoTarea,
                Predeterminado = nuevaTarea.Predeterminado,
                IdEstadoTarea = EstadoTareaDomestica.Activo // Asignar el estado de la tarea
            };

            // Agregar la nueva tarea a la base de datos
            _context.TareaDomestica.Add(tareaDomestica);
            await _context.SaveChangesAsync();

            var mensaje3 = _firebasetranslate.Traducir("Tarea doméstica creada exitosamente.", lenguaje);
            return new OperationResult(true, mensaje3);
        }

        public async Task<OperationResult> EditarTareaDomestica(int idUsuario, TareaDomesticaPostDto tareaEditada, string lenguaje = "es")
        {
            // Obtener el IdFamilia asociado al usuario
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);
                return new OperationResult(false, mensaje);
            }

            // Buscar la tarea existente que se desea editar
            var tareaDomestica = await _context.TareaDomestica
                .FirstOrDefaultAsync(t => t.IdTareaDomestica == tareaEditada.IdTareaDomestica);

            if (tareaDomestica == null)
            {
                var mensaje2 = _firebasetranslate.Traducir("Tarea no encontrada.", lenguaje);
                return new OperationResult(false, mensaje2);
            }

            // Si la tarea es predeterminada, eliminar el registro en TareasDesactivadas y actualizar el IdEstadoTarea
            if (tareaDomestica.Predeterminado )
            {
                var tareaDesactivada = await _context.TareasDesactivadas
                    .FirstOrDefaultAsync(td => td.IdTareaDomestica == tareaEditada.IdTareaDomestica && td.IdFamilia == usuario.IdFamilia);

                if (tareaDesactivada != null)
                {
                    _context.TareasDesactivadas.Remove(tareaDesactivada);

                    // Asegurarse de que el estado de la tarea sea "1" (activada)
                    tareaDomestica.IdEstadoTarea = EstadoTareaDomestica.Activo;

                    // Guardar los cambios y devolver el resultado
                    await _context.SaveChangesAsync();
                    var mensaje3 = _firebasetranslate.Traducir("Tarea predeterminada activada correctamente.", lenguaje);
                    return new OperationResult(true, mensaje3);
                }
                else
                {
                    var nuevaTareaDesactivada = new TareasDesactivadas
                    {
                        IdTareaDomestica = tareaEditada.IdTareaDomestica,
                        IdFamilia = usuario.IdFamilia.Value,
                        IdEstadoTarea = EstadoTareaDomestica.Desactivada
                    };

                    _context.TareasDesactivadas.Add(nuevaTareaDesactivada);
                    // Guardar los cambios y devolver el resultado
                    await _context.SaveChangesAsync();
                    var mensaje4 = _firebasetranslate.Traducir("Tarea predeterminada desactivada correctamente.", lenguaje);
                    return new OperationResult(true, mensaje4);
                }

               
            }
            

            // Validar que no exista otra tarea con el mismo nombre en la familia o como predeterminada, excluyendo la tarea actual
            var tareaExistente = await _context.TareaDomestica
                .FirstOrDefaultAsync(t => t.Nombre == tareaEditada.Nombre
                                          && (t.IdFamilia == usuario.IdFamilia || tareaEditada.Predeterminado)
                                          && t.IdTareaDomestica != tareaEditada.IdTareaDomestica);

            if (tareaExistente != null)
            {
                var mensaje5 = _firebasetranslate.Traducir("Ya existe una tarea con ese nombre en la familia o como predeterminada.", lenguaje);
                return new OperationResult(false, mensaje5);
            }

            // Actualizar las propiedades de la tarea doméstica con los datos editados (si no es predeterminada)
            tareaDomestica.Nombre = tareaEditada.Nombre;
            tareaDomestica.Descripcion = tareaEditada.Descripcion;
            tareaDomestica.IdTipoTarea = tareaEditada.IdTipoTarea;
            tareaDomestica.Predeterminado = tareaEditada.Predeterminado;
            tareaDomestica.IdEstadoTarea = tareaEditada.IdEstadoTarea;

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            var mensaje6 = _firebasetranslate.Traducir("Tarea doméstica editada exitosamente.", lenguaje);
            return new OperationResult(true, mensaje6);
        }

        public async Task<OperationResult> ObtenerTiposTarea(string lenguaje = "es")
        {
            try
            {
                // Obtener todos los tipos de tarea
                var tiposTareaDto = await _context.TipoTarea
                .Select(tipo => new
                {
                    IdTipoTarea = tipo.IdTipoTarea,
                    Nombre = _firebasetranslate.Traducir(tipo.Nombre, lenguaje),
                    Descripcion = _firebasetranslate.Traducir(tipo.Descripcion, lenguaje)
                })
                .ToListAsync();


                var mensaje = _firebasetranslate.Traducir("Tipos de tarea obtenidos exitosamente.", lenguaje);
                return new OperationResult(true, mensaje, tiposTareaDto);
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error al obtener tipos de tarea", lenguaje);
                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }


    }


}
