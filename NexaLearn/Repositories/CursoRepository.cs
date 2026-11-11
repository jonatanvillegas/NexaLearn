using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NexaLearn.DTOs;
using NexaLearn.Models;
using NexaLearn.Repositories.Interfaces;
using NexaLearn.Services;

namespace NexaLearn.Repositories;

public class CursoRepository : ICursoRepository
{
    private readonly NexaLearnContext _context;
    private readonly IImageSharp _imageSharp;

    public CursoRepository(NexaLearnContext context, IImageSharp imageSharp)
    {
        _context = context;
        _imageSharp = imageSharp;
    }

    public async Task<IReadOnlyList<CursoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cursos = await _context.Cursos.AsNoTracking().ToListAsync(cancellationToken);
        return cursos.Select(ToDto).ToArray();
    }

    public async Task<CursoDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var curso = await _context.Cursos.AsNoTracking().SingleOrDefaultAsync(curso => curso.Id == id, cancellationToken);
        return curso is null ? null : ToDto(curso);
    }

    public async Task<CursoDto> CreateAsync(GuardarCursoDto input, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(input, cancellationToken);
        var curso = new Curso { FechaCreacion = DateTime.UtcNow };
        Apply(curso, input);
        if (input.Imagen is not null)
        {
            curso.CursoImagen = await _imageSharp.ProcesarAsync(input.Imagen, cancellationToken);
            curso.CursoImagen.FechaRegistro = DateTime.UtcNow;
        }

        // EF guarda el curso y su imagen opcional juntos y asigna el CursoId.
        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(curso);
    }

    public async Task<bool> UpdateAsync(int id, GuardarCursoDto input, CancellationToken cancellationToken = default)
    {
        var curso = await _context.Cursos.FindAsync([id], cancellationToken);
        if (curso is null) return false;
        await ValidateAsync(input, cancellationToken);

        // Una imagen inválida no debe cambiar los datos del curso.
        if (input.Imagen is not null)
        {
            var processed = await _imageSharp.ProcesarAsync(input.Imagen, cancellationToken);
            var actual = await _context.CursoImagens.SingleOrDefaultAsync(imagen => imagen.CursoId == id, cancellationToken);
            if (actual is null)
            {
                processed.CursoId = id;
                processed.FechaRegistro = DateTime.UtcNow;
                _context.CursoImagens.Add(processed);
            }
            else
            {
                actual.NombreArchivo = processed.NombreArchivo;
                actual.MimeType = processed.MimeType;
                actual.Contenido = processed.Contenido;
            }
        }

        Apply(curso, input);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var curso = await _context.Cursos.Include(curso => curso.CursoImagen)
            .SingleOrDefaultAsync(curso => curso.Id == id, cancellationToken);
        if (curso is null) return false;
        if (await _context.Leccions.AnyAsync(leccion => leccion.CursoId == id, cancellationToken) ||
            await _context.Inscripcions.AnyAsync(inscripcion => inscripcion.CursoId == id, cancellationToken))
            throw new NexaLearn.Exceptions.ConflictoException("No se puede eliminar un curso con lecciones o inscripciones.");
        if (curso.CursoImagen is not null) _context.CursoImagens.Remove(curso.CursoImagen);
        _context.Cursos.Remove(curso);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<ArchivoImagenDto?> GetImagenAsync(int cursoId, CancellationToken cancellationToken = default)
        => _context.CursoImagens.AsNoTracking().Where(imagen => imagen.CursoId == cursoId)
            .Select(imagen => new ArchivoImagenDto(imagen.Contenido, imagen.MimeType, imagen.NombreArchivo))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task ValidateAsync(GuardarCursoDto input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        if (!await _context.Categoria.AnyAsync(categoria => categoria.Id == input.CategoriaId, cancellationToken))
            throw new ValidationException("La categoría no existe.");
    }

    private static void Apply(Curso curso, GuardarCursoDto input)
    {
        curso.CategoriaId = input.CategoriaId;
        curso.Titulo = input.Titulo.Trim();
        curso.Descripcion = input.Descripcion;
        curso.Precio = input.Precio;
        curso.Estado = input.Estado;
    }

    private static CursoDto ToDto(Curso curso) => new(curso.Id, curso.CategoriaId,
        curso.Titulo, curso.Descripcion, curso.Precio, curso.Estado, curso.FechaCreacion);
}
