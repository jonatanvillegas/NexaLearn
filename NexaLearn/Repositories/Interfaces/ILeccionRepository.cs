using NexaLearn.DTOs;

namespace NexaLearn.Repositories.Interfaces;

public interface ILeccionRepository
{
    Task<IReadOnlyList<LeccionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LeccionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LeccionDto> CreateAsync(GuardarLeccionDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarLeccionDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
