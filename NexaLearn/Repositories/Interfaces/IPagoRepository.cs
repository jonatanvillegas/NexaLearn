using NexaLearn.DTOs;

namespace NexaLearn.Repositories.Interfaces;

public interface IPagoRepository
{
    Task<IReadOnlyList<PagoDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagoDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagoDto> CreateAsync(GuardarPagoDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarPagoDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
