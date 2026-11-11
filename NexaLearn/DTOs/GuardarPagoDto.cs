using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class GuardarPagoDto
{
    [Range(1, int.MaxValue)]
    public int InscripcionId { get; set; }
    [StringLength(150)]
    public string? PayPalOrderId { get; set; }
    [StringLength(150)]
    public string? PayPalCaptureId { get; set; }
    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal Monto { get; set; }
    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string Moneda { get; set; } = "USD";
    [Required, StringLength(30)]
    public string Estado { get; set; } = "PENDIENTE";
    public DateTime? FechaPago { get; set; }
}
