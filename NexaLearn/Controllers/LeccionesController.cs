using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/lecciones")]
public class LeccionesController(IUnitOfWork unitOfWork, IHttpContextAccessor accessor) : BaseController(accessor)
{
    [HttpGet]
    public async Task<ActionResult<DataTableResponse<LeccionDto>>> GetAll([FromQuery] DataTableRequest request,
        CancellationToken cancellationToken)
        => Ok(await unitOfWork.Lecciones.GetAllAsync(request, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeccionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Lecciones.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<LeccionDto>> Create(GuardarLeccionDto input, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Lecciones.CreateAsync(input, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Update(int id, GuardarLeccionDto input, CancellationToken cancellationToken)
        => await unitOfWork.Lecciones.UpdateAsync(id, input, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await unitOfWork.Lecciones.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
