using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class GuardarPerfilDto
{
    [Required, StringLength(450)]
    public string UserId { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Apellido { get; set; } = string.Empty;
    [StringLength(20)]
    public string? Telefono { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public bool Estado { get; set; } = true;
}
