namespace NexaLearn.DTOs;

public record PagoHistorialDto(int CursoId, string Curso, string GuidId, string Cliente,
    decimal MontoPagado, string Moneda, string Estado, DateTime? FechaPago);
