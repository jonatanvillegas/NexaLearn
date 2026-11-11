namespace NexaLearn.Services;

public interface ICorreoService
{
    Task EnviarAsync(string destino, string asunto, string contenido, CancellationToken cancellationToken = default);
}
