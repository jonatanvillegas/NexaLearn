using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NexaLearn.Datatable;
using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;
using NexaLearn.Exceptions;
using NexaLearn.Models;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Repositories;

public class LeccionRepository : ILeccionRepository
{
    private readonly NexaLearnContext _context;
    private readonly DataTableService _dataTable;

    public LeccionRepository(NexaLearnContext context, DataTableService dataTable)
    {
        _context = context;
        _dataTable = dataTable;
    }

    public Task<DataTableResponse<LeccionDto>> GetAllAsync(DataTableRequest? request = null,
        CancellationToken cancellationToken = default)
        => GetPagedAsync(_context.Leccions.AsNoTracking(), request);

    public Task<DataTableResponse<LeccionDto>> GetByCursoIdAsync(int cursoId, DataTableRequest? request = null,
        CancellationToken cancellationToken = default)
        => GetPagedAsync(_context.Leccions.AsNoTracking().Where(leccion => leccion.CursoId == cursoId)
            .OrderBy(leccion => leccion.Orden), request);

    public async Task<LeccionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Leccions.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<LeccionDto> CreateAsync(GuardarLeccionDto input, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(input, 0, cancellationToken);
        var entity = new Leccion();
        Apply(entity, input);
        _context.Leccions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, GuardarLeccionDto input, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Leccions.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        await ValidateAsync(input, id, cancellationToken);
        Apply(entity, input);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Leccions.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        _context.Leccions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateAsync(GuardarLeccionDto input, int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        if (!await _context.Cursos.AnyAsync(entity => entity.Id == input.CursoId, cancellationToken))
            throw new ValidationException("El curso no existe.");
        if (await _context.Leccions.AnyAsync(entity => entity.CursoId == input.CursoId &&
            entity.Orden == input.Orden && entity.Id != id, cancellationToken))
            throw new ConflictoException("Ya existe una lección con ese orden en el curso.");
    }

    private static LeccionDto ToDto(Leccion entity) => new(entity.Id, entity.CursoId, entity.Titulo,
        entity.Descripcion, entity.Contenido, entity.VideoUrl, entity.Orden, entity.Estado);

    private Task<DataTableResponse<LeccionDto>> GetPagedAsync(IQueryable<Leccion> query, DataTableRequest? request)
        => _dataTable.GetPagedDataAsync(query.Select(leccion => new LeccionDto(leccion.Id, leccion.CursoId,
            leccion.Titulo, leccion.Descripcion, leccion.Contenido, leccion.VideoUrl, leccion.Orden,
            leccion.Estado)), request ?? new());

    private static void Apply(Leccion entity, GuardarLeccionDto input)
    {
        entity.CursoId = input.CursoId;
        entity.Titulo = input.Titulo.Trim();
        entity.Descripcion = input.Descripcion;
        entity.Contenido = input.Contenido;
        entity.VideoUrl = input.VideoUrl;
        entity.Orden = input.Orden;
        entity.Estado = input.Estado;
    }

}
