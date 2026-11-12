using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;

namespace NexaLearn.Repositories.Interfaces;

public interface ICursoImagenRepository
{
    Task<DataTableResponse<CursoImagenDto>> GetAllAsync(DataTableRequest? request = null, CancellationToken cancellationToken = default);
    Task<CursoImagenDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CursoImagenDto> CreateAsync(GuardarCursoImagenDto input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, GuardarCursoImagenDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ArchivoImagenDto?> GetArchivoAsync(int id, CancellationToken cancellationToken = default);
}
