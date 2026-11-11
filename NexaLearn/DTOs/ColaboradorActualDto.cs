namespace NexaLearn.DTOs;

public record ColaboradorActualDto(
    string? Id,
    string? Nombre,
    string? Correo,
    IReadOnlyList<string> Roles);
