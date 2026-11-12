using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/cursos")]
public class CursosController(IUnitOfWork unitOfWork, IHttpContextAccessor accessor) : BaseController(accessor)
{
    [HttpGet]
    public async Task<ActionResult<DataTableResponse<CursoDto>>> GetAll([FromQuery] DataTableRequest request,
        int? categoriaId, CancellationToken cancellationToken)
        => Ok(categoriaId is null
            ? await unitOfWork.Cursos.GetAllAsync(request, cancellationToken)
            : await unitOfWork.Cursos.GetByCategoriaIdAsync(categoriaId.Value, request, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CursoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Cursos.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("{cursoId:int}/lecciones")]
    public async Task<ActionResult<DataTableResponse<LeccionDto>>> GetLecciones(int cursoId,
        [FromQuery] DataTableRequest request, CancellationToken cancellationToken)
    {
        var curso = await unitOfWork.Cursos.GetByIdAsync(cursoId, cancellationToken);
        if (curso is null) return NotFound();
        var lecciones = await unitOfWork.Lecciones.GetByCursoIdAsync(cursoId, request, cancellationToken);
        if (!User.IsInRole(RoleNames.Admin))
        {
            lecciones.data = lecciones.data.Select(leccion => leccion with
            {
                Contenido = null,
                VideoUrl = null
            }).ToList();
        }
        return Ok(lecciones);
    }

    [HttpGet("{cursoId:int}/mis-lecciones")]
    [Authorize]
    public async Task<ActionResult<DataTableResponse<LeccionDto>>> MisLecciones(int cursoId,
        [FromQuery] DataTableRequest request, CancellationToken cancellationToken)
    {
        var userId = ColaboradorActual?.Id;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var inscripcion = await unitOfWork.Inscripciones.GetByUserAndCursoAsync(userId, cursoId, cancellationToken);
        if (!string.Equals(inscripcion?.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase)) return Forbid();

        return Ok(await unitOfWork.Lecciones.GetByCursoIdAsync(cursoId, request, cancellationToken));
    }

    [HttpGet("{cursoId:int}/mi-inscripcion")]
    [Authorize]
    public async Task<ActionResult<EstadoInscripcionCursoDto>> MiInscripcion(int cursoId,
        CancellationToken cancellationToken)
    {
        var userId = ColaboradorActual?.Id;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var curso = await unitOfWork.Cursos.GetByIdAsync(cursoId, cancellationToken);
        if (curso is null) return NotFound();

        var inscripcion = await unitOfWork.Inscripciones.GetByUserAndCursoAsync(userId, cursoId, cancellationToken);
        return Ok(new EstadoInscripcionCursoDto(inscripcion is not null, inscripcion?.Estado));
    }

    [HttpPost, Consumes("multipart/form-data")]
    [Authorize(Roles = RoleNames.Admin)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<ActionResult<CursoDto>> Create([FromForm] GuardarCursoDto input, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Cursos.CreateAsync(input, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}"), Consumes("multipart/form-data")]
    [Authorize(Roles = RoleNames.Admin)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Update(int id, [FromForm] GuardarCursoDto input, CancellationToken cancellationToken)
        => await unitOfWork.Cursos.UpdateAsync(id, input, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await unitOfWork.Cursos.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id:int}/imagen")]
    public async Task<IActionResult> GetImagen(int id, CancellationToken cancellationToken)
    {
        var image = await unitOfWork.Cursos.GetImagenAsync(id, cancellationToken);
        return image is null ? NotFound() : File(image.Contenido, image.MimeType);
    }
}
