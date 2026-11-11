using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Repositories;

// Agrupa los repositorios de la solicitud. Cada repositorio guarda su operación.
public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(
        ICategoriaRepository categorias,
        ICursoRepository cursos,
        ILeccionRepository lecciones,
        IInscripcionRepository inscripciones,
        IPagoRepository pagos,
        IPerfilRepository perfiles,
        ICursoImagenRepository cursoImagenes)
    {
        Categorias = categorias;
        Cursos = cursos;
        Lecciones = lecciones;
        Inscripciones = inscripciones;
        Pagos = pagos;
        Perfiles = perfiles;
        CursoImagenes = cursoImagenes;
    }

    public ICategoriaRepository Categorias { get; }
    public ICursoRepository Cursos { get; }
    public ILeccionRepository Lecciones { get; }
    public IInscripcionRepository Inscripciones { get; }
    public IPagoRepository Pagos { get; }
    public IPerfilRepository Perfiles { get; }
    public ICursoImagenRepository CursoImagenes { get; }
}
