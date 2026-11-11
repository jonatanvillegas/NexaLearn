using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NexaLearn.DTOs;

namespace NexaLearn.Controllers;

public abstract class BaseController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseController(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected ColaboradorActualDto? ColaboradorActual
    {
        get
        {
            // Se consulta en cada acceso, no en el constructor
            var identities = _httpContextAccessor.HttpContext?.User.Identities
                .Where(identity => identity.IsAuthenticated).ToArray();

            if (identities is null || identities.Length == 0)
            {
                return null;
            }

            var claims = identities.SelectMany(identity => identity.Claims).ToArray();
            string? ClaimValue(params string[] types)
                => types.Select(type => claims.FirstOrDefault(claim => claim.Type == type)?.Value)
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

            var roles = identities.SelectMany(identity => identity.Claims.Where(claim =>
                    claim.Type == identity.RoleClaimType ||
                    claim.Type == ClaimTypes.Role || claim.Type == "roles"))
                .Select(claim => claim.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            return new ColaboradorActualDto(
                ClaimValue(ClaimTypes.NameIdentifier, "sub", "oid",
                    "http://schemas.microsoft.com/identity/claims/objectidentifier"),
                ClaimValue(ClaimTypes.Name, "name") ?? identities.Select(identity => identity.Name)
                    .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)),
                ClaimValue(ClaimTypes.Email, "email"),
                roles);
        }
    }
}
