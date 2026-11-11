using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;
using NexaLearn.Services;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/inscripciones")]
[Authorize(Roles = RoleNames.Admin)]
public class InscripcionesController(
    IUnitOfWork unitOfWork,
    IHttpContextAccessor accessor,
    UserManager<IdentityUser> users,
    ICorreoService correoService,
    ILogger<InscripcionesController> logger) : BaseController(accessor)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InscripcionDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await unitOfWork.Inscripciones.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InscripcionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Inscripciones.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<InscripcionDto>> Create(GuardarInscripcionDto input, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Inscripciones.CreateAsync(input, cancellationToken);
        await EnviarConfirmacionAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GuardarInscripcionDto input, CancellationToken cancellationToken)
        => await unitOfWork.Inscripciones.UpdateAsync(id, input, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await unitOfWork.Inscripciones.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private async Task EnviarConfirmacionAsync(InscripcionDto inscripcion, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(inscripcion.UserId);
        if (string.IsNullOrWhiteSpace(user?.Email)) return;

        var curso = await unitOfWork.Cursos.GetByIdAsync(inscripcion.CursoId, cancellationToken);
        var titulo = curso?.Titulo ?? "el curso seleccionado";
        try
        {
            await correoService.EnviarAsync(user.Email, "Inscripción confirmada",
                $"Tu inscripción al curso {titulo} fue registrada correctamente.", cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo enviar el correo de inscripción a {Destino}.", user.Email);
        }
    }
}
