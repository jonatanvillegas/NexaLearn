using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class GuardarCategoriaDto
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; } = true;
}
