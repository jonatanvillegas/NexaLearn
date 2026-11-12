using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;

namespace NexaLearn.Repositories.Interfaces;

public interface ICategoriaRepository
{
    Task<DataTableResponse<CategoriaDto>> GetAllAsync(DataTableRequest? request = null, CancellationToken cancellationToken = default);
    Task<CategoriaDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoriaDto> CreateAsync(GuardarCategoriaDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarCategoriaDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
