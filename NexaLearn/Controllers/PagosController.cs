using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;
using NexaLearn.Services;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/pagos")]
[Authorize(Roles = RoleNames.Admin)]
public class PagosController(
    IUnitOfWork unitOfWork,
    IHttpContextAccessor accessor,
    UserManager<IdentityUser> users,
    ICorreoService correoService,
    ILogger<PagosController> logger) : BaseController(accessor)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PagoDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await unitOfWork.Pagos.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PagoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Pagos.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<PagoDto>> Create(GuardarPagoDto input, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Pagos.CreateAsync(input, cancellationToken);
        if (EstaPagado(entity.Estado)) await EnviarConfirmacionAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GuardarPagoDto input, CancellationToken cancellationToken)
    {
        var anterior = await unitOfWork.Pagos.GetByIdAsync(id, cancellationToken);
        if (anterior is null) return NotFound();
        if (!await unitOfWork.Pagos.UpdateAsync(id, input, cancellationToken)) return NotFound();

        if (!EstaPagado(anterior.Estado) && EstaPagado(input.Estado))
        {
            var pago = await unitOfWork.Pagos.GetByIdAsync(id, cancellationToken);
            if (pago is not null) await EnviarConfirmacionAsync(pago, cancellationToken);
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await unitOfWork.Pagos.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private async Task EnviarConfirmacionAsync(PagoDto pago, CancellationToken cancellationToken)
    {
        var inscripcion = await unitOfWork.Inscripciones.GetByIdAsync(pago.InscripcionId, cancellationToken);
        if (inscripcion is null) return;

        var user = await users.FindByIdAsync(inscripcion.UserId);
        if (string.IsNullOrWhiteSpace(user?.Email)) return;

        var curso = await unitOfWork.Cursos.GetByIdAsync(inscripcion.CursoId, cancellationToken);
        var titulo = curso?.Titulo ?? "el curso seleccionado";
        try
        {
            await correoService.EnviarAsync(user.Email, "Pago confirmado",
                $"Tu pago de {pago.Monto:N2} {pago.Moneda} para el curso {titulo} fue confirmado.", cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo enviar el correo de pago a {Destino}.", user.Email);
        }
    }

    private static bool EstaPagado(string estado)
        => string.Equals(estado, "PAGADO", StringComparison.OrdinalIgnoreCase);
}
