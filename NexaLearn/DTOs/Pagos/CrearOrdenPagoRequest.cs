using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs.Pagos;

public class CrearOrdenPagoRequest
{
    [Range(1, int.MaxValue)]
    public int CursoId { get; set; }
}
