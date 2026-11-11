using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class ActualizarRolDto
{
    [Required]
    public string Rol { get; set; } = string.Empty;
}
