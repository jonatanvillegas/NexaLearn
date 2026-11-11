using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NexaLearn.DTOs;
using NexaLearn.Exceptions;
using NexaLearn.Models;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly NexaLearnContext _context;

    public CategoriaRepository(NexaLearnContext context) => _context = context;

    public async Task<IReadOnlyList<CategoriaDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Categoria.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(ToDto).ToArray();
    }

    public async Task<CategoriaDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categoria.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<CategoriaDto> CreateAsync(GuardarCategoriaDto input, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(input, 0, cancellationToken);
        var entity = new Categorium();
        Apply(entity, input);
        _context.Categoria.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, GuardarCategoriaDto input, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categoria.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        await ValidateAsync(input, id, cancellationToken);
        Apply(entity, input);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categoria.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        if (await _context.Cursos.AnyAsync(curso => curso.CategoriaId == id, cancellationToken))
            throw new ConflictoException("No se puede eliminar una categoría con cursos.");
        _context.Categoria.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Task ValidateAsync(GuardarCategoriaDto input, int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        return Task.CompletedTask;
    }

    private static CategoriaDto ToDto(Categorium entity)
        => new(entity.Id, entity.Nombre, entity.Descripcion, entity.Estado);

    private static void Apply(Categorium entity, GuardarCategoriaDto input)
    {
        entity.Nombre = input.Nombre.Trim();
        entity.Descripcion = input.Descripcion;
        entity.Estado = input.Estado;
    }
}
