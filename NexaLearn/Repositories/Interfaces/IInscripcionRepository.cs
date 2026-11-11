using NexaLearn.DTOs;

namespace NexaLearn.Repositories.Interfaces;

public interface IInscripcionRepository
{
    Task<IReadOnlyList<InscripcionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InscripcionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InscripcionDto> CreateAsync(GuardarInscripcionDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarInscripcionDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
