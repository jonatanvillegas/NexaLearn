using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class GuardarLeccionDto
{
    [Range(1, int.MaxValue)]
    public int CursoId { get; set; }
    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Descripcion { get; set; }
    public string? Contenido { get; set; }
    [StringLength(500), Url]
    public string? VideoUrl { get; set; }
    [Range(1, int.MaxValue)]
    public int Orden { get; set; } = 1;
    public bool Estado { get; set; } = true;
}
