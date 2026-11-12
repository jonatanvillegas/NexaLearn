using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;

namespace NexaLearn.Repositories.Interfaces;

public interface IPerfilRepository
{
    Task<DataTableResponse<PerfilDto>> GetAllAsync(DataTableRequest? request = null, CancellationToken cancellationToken = default);
    Task<PerfilDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PerfilDto> CreateAsync(GuardarPerfilDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarPerfilDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
