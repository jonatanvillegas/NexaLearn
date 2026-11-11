namespace NexaLearn.DTOs;

public record PerfilDto(int Id, string UserId, string Nombre, string Apellido, string? Telefono,
    DateOnly? FechaNacimiento, DateTime FechaRegistro, bool Estado);
