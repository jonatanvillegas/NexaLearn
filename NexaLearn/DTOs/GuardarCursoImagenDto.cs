using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class GuardarCursoImagenDto
{
    [Range(1, int.MaxValue)]
    public int CursoId { get; set; }
    [Required]
    public IFormFile Imagen { get; set; } = null!;
}
