using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/cursoimagenes")]
public class CursoImagenesController(IUnitOfWork unitOfWork, IHttpContextAccessor accessor) : BaseController(accessor)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CursoImagenDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await unitOfWork.CursoImagenes.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CursoImagenDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.CursoImagenes.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost, Consumes("multipart/form-data")]
    [Authorize(Roles = RoleNames.Admin)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<ActionResult<CursoImagenDto>> Create([FromForm] GuardarCursoImagenDto input, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.CursoImagenes.CreateAsync(input, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}"), Consumes("multipart/form-data")]
    [Authorize(Roles = RoleNames.Admin)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Update(int id, [FromForm] GuardarCursoImagenDto input, CancellationToken cancellationToken)
        => await unitOfWork.CursoImagenes.UpdateAsync(id, input, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await unitOfWork.CursoImagenes.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id:int}/contenido")]
    public async Task<IActionResult> GetContenido(int id, CancellationToken cancellationToken)
    {
        var image = await unitOfWork.CursoImagenes.GetArchivoAsync(id, cancellationToken);
        return image is null ? NotFound() : File(image.Contenido, image.MimeType);
    }
}
