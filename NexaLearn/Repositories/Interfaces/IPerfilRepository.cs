using NexaLearn.DTOs;

namespace NexaLearn.Repositories.Interfaces;

public interface IPerfilRepository
{
    Task<IReadOnlyList<PerfilDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PerfilDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PerfilDto> CreateAsync(GuardarPerfilDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarPerfilDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
