namespace NexaLearn.DTOs.Pagos;

public record CapturarOrdenPagoResponse(int PagoId, int CursoId, string Estado, string? CaptureId);
