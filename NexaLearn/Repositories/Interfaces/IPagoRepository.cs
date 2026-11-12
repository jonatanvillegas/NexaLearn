using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;

namespace NexaLearn.Repositories.Interfaces;

public interface IPagoRepository
{
    Task<DataTableResponse<PagoHistorialDto>> GetAllAsync(DataTableRequest? request = null, CancellationToken cancellationToken = default);
    Task<PagoDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagoDto> CreateAsync(GuardarPagoDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarPagoDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
