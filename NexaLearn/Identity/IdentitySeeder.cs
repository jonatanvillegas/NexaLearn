using Microsoft.AspNetCore.Identity;

namespace NexaLearn.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { RoleNames.Admin, RoleNames.Cliente })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"No se pudo crear el rol {role}: {string.Join(", ", result.Errors.Select(error => error.Description))}");
            }
        }

        var email = configuration["Identity:AdministradorInicial:Email"];
        var password = configuration["Identity:AdministradorInicial:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password)) return;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configura Identity:AdministradorInicial:Email y Password juntos.");

        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var admin = await users.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"No se pudo crear el administrador inicial: {string.Join(", ", result.Errors.Select(error => error.Description))}");
        }

        if (!await users.IsInRoleAsync(admin, RoleNames.Admin))
        {
            var result = await users.AddToRoleAsync(admin, RoleNames.Admin);
            if (!result.Succeeded)
                throw new InvalidOperationException($"No se pudo asignar el rol Admin: {string.Join(", ", result.Errors.Select(error => error.Description))}");
        }
    }
}
