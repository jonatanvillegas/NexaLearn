using NexaLearn.DTOs;

namespace NexaLearn.Repositories.Interfaces;

public interface ICursoRepository
{
    Task<IReadOnlyList<CursoDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CursoDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CursoDto> CreateAsync(GuardarCursoDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarCursoDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ArchivoImagenDto?> GetImagenAsync(int cursoId, CancellationToken cancellationToken = default);
}
