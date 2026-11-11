using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/perfiles")]
[Authorize(Roles = RoleNames.Admin)]
public class PerfilesController(IUnitOfWork unitOfWork, IHttpContextAccessor accessor) : BaseController(accessor)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PerfilDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await unitOfWork.Perfiles.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PerfilDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Perfiles.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<PerfilDto>> Create(GuardarPerfilDto input, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Perfiles.CreateAsync(input, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GuardarPerfilDto input, CancellationToken cancellationToken)
        => await unitOfWork.Perfiles.UpdateAsync(id, input, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await unitOfWork.Perfiles.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
