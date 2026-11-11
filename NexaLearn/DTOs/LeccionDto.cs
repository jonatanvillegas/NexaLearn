namespace NexaLearn.DTOs;

public record LeccionDto(int Id, int CursoId, string Titulo, string? Descripcion,
    string? Contenido, string? VideoUrl, int Orden, bool Estado);
