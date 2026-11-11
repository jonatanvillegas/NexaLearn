using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/cursos")]
public class CursosController(IUnitOfWork unitOfWork, IHttpContextAccessor accessor) : BaseController(accessor)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CursoDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await unitOfWork.Cursos.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CursoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Cursos.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
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
