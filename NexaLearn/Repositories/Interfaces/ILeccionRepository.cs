using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;

namespace NexaLearn.Repositories.Interfaces;

public interface ILeccionRepository
{
    Task<DataTableResponse<LeccionDto>> GetAllAsync(DataTableRequest? request = null, CancellationToken cancellationToken = default);
    Task<DataTableResponse<LeccionDto>> GetByCursoIdAsync(int cursoId, DataTableRequest? request = null, CancellationToken cancellationToken = default);
    Task<LeccionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LeccionDto> CreateAsync(GuardarLeccionDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarLeccionDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
