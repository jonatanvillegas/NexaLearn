using System.ComponentModel.DataAnnotations;

namespace NexaLearn.DTOs;

public class GuardarCursoDto
{
    [Range(1, int.MaxValue)]
    public int CategoriaId { get; set; }
    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal Precio { get; set; }
    public bool Estado { get; set; } = true;
    public IFormFile? Imagen { get; set; }
}
