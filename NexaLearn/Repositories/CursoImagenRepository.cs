using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NexaLearn.DTOs;
using NexaLearn.Exceptions;
using NexaLearn.Models;
using NexaLearn.Repositories.Interfaces;
using NexaLearn.Services;

namespace NexaLearn.Repositories;

public class CursoImagenRepository : ICursoImagenRepository
{
    private readonly NexaLearnContext _context;
    private readonly IImageSharp _imageSharp;

    public CursoImagenRepository(NexaLearnContext context, IImageSharp imageSharp)
    {
        _context = context;
        _imageSharp = imageSharp;
    }

    public async Task<IReadOnlyList<CursoImagenDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.CursoImagens.AsNoTracking()
            .Select(imagen => new CursoImagenDto(imagen.Id, imagen.CursoId, imagen.NombreArchivo,
                imagen.MimeType, imagen.Contenido.Length, imagen.FechaRegistro)).ToListAsync(cancellationToken);

    public Task<CursoImagenDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.CursoImagens.AsNoTracking().Where(imagen => imagen.Id == id)
            .Select(imagen => new CursoImagenDto(imagen.Id, imagen.CursoId, imagen.NombreArchivo,
                imagen.MimeType, imagen.Contenido.Length, imagen.FechaRegistro)).SingleOrDefaultAsync(cancellationToken);

    public async Task<CursoImagenDto> CreateAsync(GuardarCursoImagenDto input, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(input, 0, cancellationToken);
        var imagen = await _imageSharp.ProcesarAsync(input.Imagen, cancellationToken);
        imagen.CursoId = input.CursoId;
        imagen.FechaRegistro = DateTime.UtcNow;
        _context.CursoImagens.Add(imagen);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(imagen);
    }

    public async Task<bool> UpdateAsync(int id, GuardarCursoImagenDto input, CancellationToken cancellationToken = default)
    {
        var imagen = await _context.CursoImagens.FindAsync([id], cancellationToken);
        if (imagen is null) return false;
        await ValidateAsync(input, id, cancellationToken);
        var processed = await _imageSharp.ProcesarAsync(input.Imagen, cancellationToken);
        imagen.CursoId = input.CursoId;
        imagen.NombreArchivo = processed.NombreArchivo;
        imagen.MimeType = processed.MimeType;
        imagen.Contenido = processed.Contenido;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var imagen = await _context.CursoImagens.FindAsync([id], cancellationToken);
        if (imagen is null) return false;
        _context.CursoImagens.Remove(imagen);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<ArchivoImagenDto?> GetArchivoAsync(int id, CancellationToken cancellationToken = default)
        => _context.CursoImagens.AsNoTracking().Where(imagen => imagen.Id == id)
            .Select(imagen => new ArchivoImagenDto(imagen.Contenido, imagen.MimeType, imagen.NombreArchivo))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task ValidateAsync(GuardarCursoImagenDto input, int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        if (!await _context.Cursos.AnyAsync(curso => curso.Id == input.CursoId, cancellationToken))
            throw new ValidationException("El curso no existe.");
        if (await _context.CursoImagens.AnyAsync(imagen => imagen.CursoId == input.CursoId && imagen.Id != id, cancellationToken))
            throw new ConflictoException("El curso ya tiene una imagen. Utiliza PUT para reemplazarla.");
    }

    private static CursoImagenDto ToDto(CursoImagen imagen) => new(imagen.Id, imagen.CursoId,
        imagen.NombreArchivo, imagen.MimeType, imagen.Contenido.Length, imagen.FechaRegistro);
}
