namespace NexaLearn.DTOs;

public record PagoDto(int Id, int InscripcionId, string? PayPalOrderId, string? PayPalCaptureId,
    decimal Monto, string Moneda, string Estado, DateTime? FechaPago);
