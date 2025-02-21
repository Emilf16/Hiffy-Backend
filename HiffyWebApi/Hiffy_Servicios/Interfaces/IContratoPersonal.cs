using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Interfaces
{
    public interface IContratoPersonal
    {
        // Obtener todos los contratos de una familia

        // Crear un nuevo contrato personal
        Task<OperationResult> CrearContratoPersonal(PostContratoDto contratoPersonal, int idFamilia, int idUsuario, string lenguaje = "es");

        // Aceptar contrato personal (asignar código de verificación)
        Task<OperationResult> AceptarContratoPersonal(int idContrato, string lenguaje = "es");

        // Comenzar contrato personal (poner estado en 'EnCurso' y solicitar código de verificación)
        Task<OperationResult> ComenzarContratoPersonal(int idContrato, int codigoVerificacion, string lenguaje = "es");

        // Cancelar contrato personal (solo si está en estado 'Pendiente' o 'Aceptado')
        Task<OperationResult> CancelarContratoPersonalVendedor(int idContrato, string motivo, string lenguaje = "es");
        Task<OperationResult> CancelarContratoPersonalFamilia(int idContrato, string motivo, string lenguaje = "es"); 
        Task<OperationResult> FinalizarContratoPersonal(int idContrato, int codigoFinalizacion, string lenguaje = "es");

        // Obtener código de verificación de un contrato personal
        Task<OperationResult> ObtenerCodigoVerificacion(int idContrato, string lenguaje = "es");

        // Obtener código de finalización de un contrato personal
        Task<OperationResult> ObtenerCodigoFinalizacion(int idContrato, string lenguaje = "es");

        // Generar un código de verificación único
        Task<int> GenerarCodigoVerificacionUnico(int idServicioContratado, string lenguaje = "es");
    }
}
