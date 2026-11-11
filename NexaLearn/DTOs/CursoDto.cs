namespace NexaLearn.DTOs;

public record CursoDto(int Id, int CategoriaId, string Titulo, string? Descripcion,
    decimal Precio, bool Estado, DateTime FechaCreacion);
