using NexaLearn.Models;

namespace NexaLearn.Services;

public interface IImageSharp
{
    Task<CursoImagen> ProcesarAsync(IFormFile imagen, CancellationToken cancellationToken = default);
}
