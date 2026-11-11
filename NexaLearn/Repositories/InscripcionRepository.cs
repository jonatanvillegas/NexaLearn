using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NexaLearn.DTOs;
using NexaLearn.Exceptions;
using NexaLearn.Models;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Repositories;

public class InscripcionRepository : IInscripcionRepository
{
    private readonly NexaLearnContext _context;

    public InscripcionRepository(NexaLearnContext context) => _context = context;

    public async Task<IReadOnlyList<InscripcionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Inscripcions.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(ToDto).ToArray();
    }

    public async Task<InscripcionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Inscripcions.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<InscripcionDto> CreateAsync(GuardarInscripcionDto input, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(input, 0, cancellationToken);
        var entity = new Inscripcion();
        Apply(entity, input);
        _context.Inscripcions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, GuardarInscripcionDto input, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Inscripcions.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        await ValidateAsync(input, id, cancellationToken);
        Apply(entity, input);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Inscripcions.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        if (await _context.Pagos.AnyAsync(pago => pago.InscripcionId == id, cancellationToken))
            throw new ConflictoException("No se puede eliminar una inscripción con pagos.");
        _context.Inscripcions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateAsync(GuardarInscripcionDto input, int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        if (!await _context.Cursos.AnyAsync(entity => entity.Id == input.CursoId, cancellationToken))
            throw new ValidationException("El curso no existe.");
        var userId = input.UserId.Trim();
        if (await _context.Inscripcions.AnyAsync(entity => entity.UserId == userId &&
            entity.CursoId == input.CursoId && entity.Id != id, cancellationToken))
            throw new ConflictoException("El usuario ya está inscrito en este curso.");
    }

    private static InscripcionDto ToDto(Inscripcion entity)
        => new(entity.Id, entity.UserId, entity.CursoId, entity.FechaInscripcion, entity.Estado);

    private static void Apply(Inscripcion entity, GuardarInscripcionDto input)
    {
        entity.UserId = input.UserId.Trim();
        entity.CursoId = input.CursoId;
        entity.Estado = input.Estado.Trim();
        if (entity.Id == 0) entity.FechaInscripcion = DateTime.UtcNow;
    }

}
