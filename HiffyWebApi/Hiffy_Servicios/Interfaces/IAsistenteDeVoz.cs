using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos; 

namespace Hiffy_Servicios.Interfaces
{
    public interface IAsistenteDeVoz
    {
        Task<OperationResult> PutActualizarDatosDispositivo(PutDispositivoFamilia dispositivo, int usuarioId, string lenguaje = "es");
        Task<OperationResult> DeleteDispositivoFamilia(string idDispositivo, int usuarioId, string lenguaje = "es"); 
        Task<OperationResult> GetMisDispositivos(  int usuarioId, string lenguaje = "es");

        //METODOS DESDE DISPOSITIVO
        Task<OperationResult> GetInformacionFamiliar(string idDispositivo, string lenguaje = "es");
        Task<OperationResult> PostRegistrarDispositivo(PostDispositivoDto dto, int codigoFamilia, string lenguaje = "es");
        Task<OperationResult> GetAsignacionesPorFecha(string idDispositivo, DateTime fechaHora, string lenguaje = "es");
        Task<OperationResult> PostAsignacion(string idDispositivo, TareaAsignadaDto dto, string lenguaje = "es");
        Task<OperationResult> DeleteAsignacionPorId(string idDispositivo, int idAsignacion, string lenguaje = "es");
        Task<OperationResult> DeleteAllAsignaciones(string idDispositivo, string lenguaje = "es");
    }
    

}
