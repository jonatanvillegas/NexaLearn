using NexaLearn.DTOs;

namespace NexaLearn.Repositories.Interfaces;

public interface ICursoImagenRepository
{
    Task<IReadOnlyList<CursoImagenDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CursoImagenDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CursoImagenDto> CreateAsync(GuardarCursoImagenDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarCursoImagenDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ArchivoImagenDto?> GetArchivoAsync(int id, CancellationToken cancellationToken = default);
}
