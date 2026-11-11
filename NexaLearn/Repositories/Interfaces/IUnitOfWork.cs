namespace NexaLearn.Repositories.Interfaces;

public interface IUnitOfWork
{
    ICategoriaRepository Categorias { get; }
    ICursoRepository Cursos { get; }
    ILeccionRepository Lecciones { get; }
    IInscripcionRepository Inscripciones { get; }
    IPagoRepository Pagos { get; }
    IPerfilRepository Perfiles { get; }
    ICursoImagenRepository CursoImagenes { get; }
}
