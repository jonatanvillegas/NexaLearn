namespace NexaLearn.DTOs.Pagos;

public record ReanudarOrdenPagoResponse(bool Completado, int CursoId, string Estado, string? ApprovalUrl);
