namespace NexaLearn.DTOs;

public record ArchivoImagenDto(byte[] Contenido, string MimeType, string? NombreArchivo);
