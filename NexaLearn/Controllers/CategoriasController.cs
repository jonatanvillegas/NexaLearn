using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : BaseController
{
    private readonly ICategoriaRepository _repository;

    public CategoriasController(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
        _repository = unitOfWork.Categorias;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _repository.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var categoria = await _repository.GetByIdAsync(id, cancellationToken);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<CategoriaDto>> Create(GuardarCategoriaDto dto, CancellationToken cancellationToken)
    {
        var categoria = await _repository.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Update(int id, GuardarCategoriaDto dto, CancellationToken cancellationToken)
        => await _repository.UpdateAsync(id, dto, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await _repository.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
