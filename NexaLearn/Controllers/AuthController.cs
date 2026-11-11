using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NexaLearn.DTOs;
using NexaLearn.Identity;
using NexaLearn.Repositories.Interfaces;
using NexaLearn.Services;

namespace NexaLearn.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<IdentityUser> users,
    IConfiguration configuration,
    IUnitOfWork unitOfWork,
    ICorreoService correoService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDto>> Registro(RegistroDto input, CancellationToken cancellationToken)
    {
        var email = input.Email.Trim().ToLowerInvariant();
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email
        };
        var result = await users.CreateAsync(user, input.Password);
        if (!result.Succeeded)
            return BadRequest(new { errores = result.Errors.Select(error => error.Description) });

        var roleResult = await users.AddToRoleAsync(user, RoleNames.Cliente);
        if (!roleResult.Succeeded)
        {
            await users.DeleteAsync(user);
            return BadRequest(new { errores = roleResult.Errors.Select(error => error.Description) });
        }

        try
        {
            await unitOfWork.Perfiles.CreateAsync(new GuardarPerfilDto
            {
                UserId = user.Id,
                Nombre = input.Nombre,
                Apellido = input.Apellido
            }, cancellationToken);
        }
        catch
        {
            await users.DeleteAsync(user);
            throw;
        }

        await EnviarCorreoAsync(email, "Bienvenido a NexaLearn",
            $"Hola {input.Nombre.Trim()}, tu usuario fue creado correctamente. Ya puedes iniciar sesión.", cancellationToken);
        return CreatedAtAction(nameof(Actual), await CrearTokenAsync(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDto>> Login(LoginDto input)
    {
        var user = await users.FindByEmailAsync(input.Email.Trim());
        if (user is null || !await users.CheckPasswordAsync(user, input.Password))
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

        return Ok(await CrearTokenAsync(user));
    }

    [HttpGet("actual")]
    [Authorize]
    public async Task<ActionResult<object>> Actual()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        return Ok(new
        {
            user.Id,
            user.Email,
            Roles = await users.GetRolesAsync(user)
        });
    }

    [HttpPut("usuarios/{userId}/rol")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> ActualizarRol(string userId, ActualizarRolDto input)
    {
        if (input.Rol is not (RoleNames.Admin or RoleNames.Cliente))
            return BadRequest(new { mensaje = "El rol debe ser Admin o Cliente." });

        var user = await users.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var rolesActuales = await users.GetRolesAsync(user);
        var remove = await users.RemoveFromRolesAsync(user, rolesActuales);
        if (!remove.Succeeded) return BadRequest(new { errores = remove.Errors.Select(error => error.Description) });

        var add = await users.AddToRoleAsync(user, input.Rol);
        if (!add.Succeeded) return BadRequest(new { errores = add.Errors.Select(error => error.Description) });
        return NoContent();
    }

    private async Task<TokenDto> CrearTokenAsync(IdentityUser user)
    {
        var jwt = configuration.GetSection("Jwt");
        var expiration = DateTime.UtcNow.AddHours(jwt.GetValue("ExpiraEnHoras", 8));
        var roles = await users.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiration,
            user.Id, user.Email ?? string.Empty, roles.ToArray());
    }

    private async Task EnviarCorreoAsync(string destino, string asunto, string contenido, CancellationToken cancellationToken)
    {
        try
        {
            await correoService.EnviarAsync(destino, asunto, contenido, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo enviar el correo de bienvenida a {Destino}.", destino);
        }
    }
}
