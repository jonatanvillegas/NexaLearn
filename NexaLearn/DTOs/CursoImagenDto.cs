namespace NexaLearn.DTOs;

public record CursoImagenDto(int Id, int CursoId, string? NombreArchivo, string MimeType,
    int TamanoBytes, DateTime FechaRegistro);
