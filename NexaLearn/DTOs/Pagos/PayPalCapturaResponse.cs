namespace NexaLearn.DTOs.Pagos;

public record PayPalCapturaResponse(string OrderId, string Estado, string? CaptureId,
    string? EstadoCaptura, string? Referencia, decimal Monto, string Moneda);
