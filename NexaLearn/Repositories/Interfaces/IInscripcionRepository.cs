using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;

namespace NexaLearn.Repositories.Interfaces;

public interface IInscripcionRepository
{
    Task<DataTableResponse<InscripcionHistorialDto>> GetAllAsync(DataTableRequest? request = null, CancellationToken cancellationToken = default);
    Task<InscripcionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InscripcionDto?> GetByUserAndCursoAsync(string userId, int cursoId, CancellationToken cancellationToken = default);
    Task<InscripcionDto> CreateAsync(GuardarInscripcionDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarInscripcionDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
