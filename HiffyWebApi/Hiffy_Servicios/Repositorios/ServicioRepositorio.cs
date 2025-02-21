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

namespace Hiffy_Servicios.Repositorios
{
    public class ServicioRepositorio : IServicio
    {
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;

        public ServicioRepositorio(AppDbContext context, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _firebasetranslate = firebasetranslate;

        }

        public async Task<OperationResult> CrearServicio(PostServicioDto servicioModel, string lenguaje = "es")
        {
            // Consulta el usuario en la base de datos
            var usuario = await _context.Usuario
                .Where(u => u.IdUsuario == servicioModel.IdUsuario)
                .Select(u => new { u.Altitud, u.Longitud })
                .FirstOrDefaultAsync();

            if(usuario == null)
            {
                var mensajeError = _firebasetranslate.Traducir(
                    "Usuario no registrado en el sistema.",
                    lenguaje
                );
                return new OperationResult(false, mensajeError);

            }
            // Verifica si la latitud o longitud son nulas
            if ( string.IsNullOrEmpty(usuario.Altitud) || string.IsNullOrEmpty(usuario.Longitud))
            {
                var mensajeError = _firebasetranslate.Traducir(
                    "Debe agregar su ubicación en la pantalla de configuración antes de crear un servicio.",
                    lenguaje
                );
                return new OperationResult(false, mensajeError);
            }

            // Crear el nuevo servicio
            var servicio = new Servicio
            {
                IdUsuario = servicioModel.IdUsuario,
                IdTipoServicio = servicioModel.IdTipoServicio,
                Nombre = servicioModel.Nombre,
                Descripcion = servicioModel.Descripcion,
                Precio = servicioModel.Precio,
                Disponibilidad = servicioModel.Disponibilidad,
                FechaPublicacion = DateTime.Now,
            };

            // Agregar el servicio a la base de datos
            await _context.Servicio.AddAsync(servicio);
            await _context.SaveChangesAsync();

            // Traducir el mensaje de éxito
            var mensajeExito = _firebasetranslate.Traducir("Servicio creado exitosamente.", lenguaje);

            return new OperationResult(true, mensajeExito, servicio.IdServicio);
        }


        public async Task<OperationResult> ActualizarServicio(PostServicioDto servicioModel, int idServicio, string lenguaje = "es")
        {
            if (idServicio != servicioModel.IdServicio)
            {
                var mensaje = _firebasetranslate.Traducir("El ID del servicio no coincide con el modelo.", lenguaje);

                return new OperationResult(false, mensaje, servicioModel.IdServicio);
            }

            var servicio = await _context.Servicio
                .Where(x => x.IdServicio == idServicio)
                .FirstOrDefaultAsync();

            if (servicio == null)
            {
                var mensaje = _firebasetranslate.Traducir("Servicio no encontrado.", lenguaje);

                return new OperationResult(false, mensaje, 0);
            }

            servicio.IdTipoServicio = servicioModel.IdTipoServicio;
            servicio.Nombre = servicioModel.Nombre;
            servicio.Descripcion = servicioModel.Descripcion;
            servicio.Precio = servicioModel.Precio;
            servicio.Disponibilidad = servicioModel.Disponibilidad; 

            await _context.SaveChangesAsync();

            var mensaje2 = _firebasetranslate.Traducir("Servicio actualizado exitosamente.", lenguaje);


            return new OperationResult(true, mensaje2, servicio.IdServicio);
        }

        public async Task<OperationResult> EliminarServicio(int idServicio, string lenguaje = "es")
        {
            // Verificar si el servicio existe
            var servicio = await _context.Servicio
                .Where(x => x.IdServicio == idServicio)
                .FirstOrDefaultAsync();

            if (servicio == null)
            {
                var mensaje = _firebasetranslate.Traducir("Servicio no encontrado.", lenguaje);

                return new OperationResult(false, mensaje, 0);
            }

            // Verificar si existen contratos en la tabla ContratoPersonal con estado "EnCurso" o "Aceptado"
            var contratosActivos = await _context.ContratoPersonal
                .Where(x => x.IdServicioContratado == idServicio &&
                            (x.Estado == EstadoContrato.EnCurso || x.Estado == EstadoContrato.Aceptado))
                .AnyAsync();

            if (contratosActivos)
            {
                var mensaje = _firebasetranslate.Traducir("No se puede eliminar el servicio porque tiene contratos activos en curso o aceptados.", lenguaje);

                return new OperationResult(false, mensaje, 0);
            }

            // Eliminar el servicio si no tiene contratos en estado "EnCurso" o "Aceptado"
            _context.Servicio.Remove(servicio);
            await _context.SaveChangesAsync();

            var mensaje2 = _firebasetranslate.Traducir("Servicio eliminado exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2, idServicio);
        }

        public async Task<OperationResult> GuardarUbicacionVendedor(int usuarioId, string latitud, string longitud, string lenguaje = "es")
        {
            // Busca al usuario por su ID
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario == null)
            {
                // Usuario no encontrado
                var mensajeError = _firebasetranslate.Traducir(
                    "El usuario no fue encontrado. Verifique el ID del usuario.",
                    lenguaje
                );
                return new OperationResult(false, mensajeError);
            }

            // Actualiza los valores de latitud y longitud
            usuario.Altitud = latitud;
            usuario.Longitud = longitud;

            // Guarda los cambios en la base de datos
            _context.Usuario.Update(usuario);
            await _context.SaveChangesAsync();

            // Mensaje de éxito
            var mensajeExito = _firebasetranslate.Traducir(
                "Ubicación guardada exitosamente.",
                lenguaje
            );

            return new OperationResult(true, mensajeExito);
        }



        public async Task<OperationResult> GetServicioPorId(int idServicio, string lenguaje = "es")
        {
            var servicio = await _context.Servicio
                .Where(x => x.IdServicio == idServicio)
                .Select(x => new GetServicioDto
                {
                    IdServicio = x.IdServicio,
                    IdUsuario = x.IdUsuario,
                    IdTipoServicio = x.IdTipoServicio,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Precio = x.Precio,
                    Disponibilidad = x.Disponibilidad,
                    FechaPublicacion = x.FechaPublicacion,
                    VendedorFoto = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.FotoUrl).FirstOrDefault(),
                    VendedorNombre = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.Nombre).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (servicio == null)
            {
                var mensaje = _firebasetranslate.Traducir("Servicio no encontrado.", lenguaje);

                return new OperationResult(false, mensaje, 0);
            }

            var mensaje2 = _firebasetranslate.Traducir("Servicio encontrado.", lenguaje);

            return new OperationResult(true, mensaje2, servicio);
        }

        public async Task<IEnumerable<GetServicioDto>> GetServicios(string? name,  string? vendorName, string? descripcion, decimal? priceMin, decimal? primeMax, int? typeServiceId, DateTime? releaseDateFrom, DateTime? releaseDateTo,int? valoracionDesde, int? valoracionHasta, string lenguaje = "es")
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "";
            }
            if (string.IsNullOrEmpty(vendorName))
            {
                vendorName = "";
            }
            if (string.IsNullOrEmpty(descripcion))
            {
                descripcion = "";
            }
            var servicios = await _context.Servicio
                 .Where(x => x.Nombre.Trim().ToUpper().Contains(name.Trim().ToUpper())
                && x.Descripcion.Trim().ToUpper().Contains(descripcion.Trim().ToUpper())
                && x.IdTipoServicio == (typeServiceId == null ? x.IdTipoServicio : typeServiceId)
                && x.FechaPublicacion.Date >= (releaseDateFrom == null ? x.FechaPublicacion.Date : releaseDateFrom.Value.Date)
                && x.FechaPublicacion.Date <= (releaseDateTo == null ? x.FechaPublicacion.Date : releaseDateTo.Value.Date)
                && x.Precio >= (priceMin == null ? x.Precio : priceMin.Value)
                && x.Precio <= (primeMax == null ? x.Precio : primeMax.Value)
                 && _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Any(u => u.Nombre.Trim().ToUpper().Contains(vendorName.Trim().ToUpper())))
                .Select(x => new GetServicioDto
                {
                    IdServicio = x.IdServicio,
                    IdUsuario = x.IdUsuario,
                    IdTipoServicio = x.IdTipoServicio,
                    
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Precio = x.Precio,
                    Disponibilidad = x.Disponibilidad,
                    FechaPublicacion = x.FechaPublicacion,
                    VendedorFoto = _context.Usuario.Where(u=> u.IdUsuario == x.IdUsuario).Select(u=> u.FotoUrl).FirstOrDefault(),
                    VendedorNombre = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.Nombre).FirstOrDefault(),
                    VendedorCorreo = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.Correo).FirstOrDefault(),
                    Valoracion = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.Valoracion.ToString()).FirstOrDefault(),
                    Longitud = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.Longitud).FirstOrDefault() ?? "",
                    Latitud = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.Altitud).FirstOrDefault()?? "",
                })
                .ToListAsync();

            

            return servicios;
        }

        public async Task<IEnumerable<GetServicioDto>> GetServiciosDelVendedor(string nombre, int usuarioId)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                nombre = "";
            }

            var servicios = await _context.Servicio
                .Where(x => x.Nombre.Trim().ToUpper().Contains(nombre.Trim().ToUpper()) && x.IdUsuario == usuarioId)
                .Select(x => new GetServicioDto
                {
                    IdServicio = x.IdServicio,
                    IdUsuario = x.IdUsuario,
                    IdTipoServicio = x.IdTipoServicio,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Precio = x.Precio,
                    Disponibilidad = x.Disponibilidad,
                    FechaPublicacion = x.FechaPublicacion,
                    VendedorFoto = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.FotoUrl).FirstOrDefault(),
                    VendedorNombre = _context.Usuario.Where(u => u.IdUsuario == x.IdUsuario).Select(u => u.Nombre).FirstOrDefault()
                })
                .ToListAsync();

            return servicios;
        }
    }
}