namespace NexaLearn.DTOs.Pagos;

public record PagoDetalleResponse(int PagoId, int CursoId, string Curso, decimal Monto,
    string Moneda, string Estado, string? OrderId, string? CaptureId, DateTime? FechaPago);
