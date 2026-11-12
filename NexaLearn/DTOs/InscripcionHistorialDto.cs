namespace NexaLearn.DTOs;

public record InscripcionHistorialDto(int InscripcionId, string UserId, string NombreCompleto,
    int CursoId, string Titulo, int CategoriaId, string Categoria, DateTime FechaInscripcion);
