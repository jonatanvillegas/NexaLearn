using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class GuardarInscripcionDto
{
    [Required, StringLength(450)]
    public string UserId { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int CursoId { get; set; }
    [Required, StringLength(30)]
    public string Estado { get; set; } = "PENDIENTE";
}
