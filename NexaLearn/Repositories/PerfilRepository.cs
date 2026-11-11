using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NexaLearn.DTOs;
using NexaLearn.Exceptions;
using NexaLearn.Models;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Repositories;

public class PerfilRepository : IPerfilRepository
{
    private readonly NexaLearnContext _context;

    public PerfilRepository(NexaLearnContext context) => _context = context;

    public async Task<IReadOnlyList<PerfilDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Perfils.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(ToDto).ToArray();
    }

    public async Task<PerfilDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Perfils.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<PerfilDto> CreateAsync(GuardarPerfilDto input, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(input, 0, cancellationToken);
        var entity = new Perfil();
        Apply(entity, input);
        _context.Perfils.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, GuardarPerfilDto input, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Perfils.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        await ValidateAsync(input, id, cancellationToken);
        Apply(entity, input);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Perfils.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        _context.Perfils.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateAsync(GuardarPerfilDto input, int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        var userId = input.UserId.Trim();
        if (await _context.Perfils.AnyAsync(entity => entity.UserId == userId && entity.Id != id, cancellationToken))
            throw new ConflictoException("El usuario ya tiene un perfil.");
    }

    private static PerfilDto ToDto(Perfil entity) => new(entity.Id, entity.UserId, entity.Nombre,
        entity.Apellido, entity.Telefono, entity.FechaNacimiento, entity.FechaRegistro, entity.Estado);

    private static void Apply(Perfil entity, GuardarPerfilDto input)
    {
        entity.UserId = input.UserId.Trim();
        entity.Nombre = input.Nombre.Trim();
        entity.Apellido = input.Apellido.Trim();
        entity.Telefono = input.Telefono;
        entity.FechaNacimiento = input.FechaNacimiento;
        entity.Estado = input.Estado;
        if (entity.Id == 0) entity.FechaRegistro = DateTime.UtcNow;
    }

}
