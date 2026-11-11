using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs.Pagos;

public class CapturarOrdenPagoRequest
{
    [Required, StringLength(150)]
    public string OrderId { get; set; } = string.Empty;
}
