namespace NexaLearn.DTOs;

public record InscripcionDto(int Id, string UserId, int CursoId, DateTime FechaInscripcion, string Estado);
