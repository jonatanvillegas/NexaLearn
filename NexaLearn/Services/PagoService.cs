using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexaLearn.DTOs.Pagos;
using NexaLearn.Exceptions;
using NexaLearn.Models;

namespace NexaLearn.Services;

public sealed class PagoService(
    NexaLearnContext context,
    IPayPalService payPal,
    IConfiguration configuration,
    UserManager<IdentityUser> users,
    ICorreoService correoService,
    ILogger<PagoService> logger) : IPagoService
{
    public async Task<CrearOrdenPagoResponse> CrearOrdenAsync(string userId, CrearOrdenPagoRequest input,
        CancellationToken cancellationToken = default)
    {
        var curso = await context.Cursos.AsNoTracking().SingleOrDefaultAsync(curso => curso.Id == input.CursoId,
            cancellationToken) ?? throw new KeyNotFoundException("El curso no existe.");
        if (!curso.Estado) throw new ValidationException("El curso no está disponible.");
        if (curso.Precio <= 0) throw new ValidationException("El curso no tiene un precio válido.");

        var inscripcionActual = await context.Inscripcions.Include(inscripcion => inscripcion.Pagos)
            .SingleOrDefaultAsync(inscripcion => inscripcion.UserId == userId && inscripcion.CursoId == input.CursoId,
                cancellationToken);
        if (inscripcionActual is not null)
        {
            if (string.Equals(inscripcionActual.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
                throw new ConflictoException("Ya tienes una inscripción activa en este curso.");
            if (inscripcionActual.Pagos.Any(pago => string.Equals(pago.Estado, "PENDIENTE", StringComparison.OrdinalIgnoreCase)))
                throw new ConflictoException("Ya tienes una orden de pago pendiente para este curso.");
            throw new ConflictoException("Ya existe una inscripción para este curso.");
        }

        var moneda = (configuration["PayPal:Currency"] ?? "USD").Trim().ToUpperInvariant();
        if (moneda.Length != 3) throw new ValidationException("PayPal:Currency debe tener tres caracteres.");
        var referencia = $"CURSO-{curso.Id}-USUARIO-{userId}";
        var orden = await payPal.CrearOrdenAsync(curso.Precio, moneda, referencia, cancellationToken);
        if (orden.Monto != curso.Precio || !string.Equals(orden.Moneda, moneda, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(orden.Referencia, referencia, StringComparison.Ordinal))
            throw new PayPalException("PayPal devolvió una orden con datos diferentes a los solicitados.");
        if (string.IsNullOrWhiteSpace(orden.ApprovalUrl))
            throw new PayPalException("PayPal no devolvió el enlace de aprobación.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var inscripcion = new Inscripcion
        {
            UserId = userId,
            CursoId = curso.Id,
            Estado = "PENDIENTE",
            FechaInscripcion = DateTime.UtcNow
        };
        context.Inscripcions.Add(inscripcion);
        await context.SaveChangesAsync(cancellationToken);

        context.Pagos.Add(new Pago
        {
            InscripcionId = inscripcion.Id,
            PayPalOrderId = orden.OrderId,
            Monto = curso.Precio,
            Moneda = moneda,
            Estado = "PENDIENTE"
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CrearOrdenPagoResponse(orden.OrderId, orden.ApprovalUrl);
    }

    public async Task<CapturarOrdenPagoResponse> CapturarOrdenAsync(string userId, CapturarOrdenPagoRequest input,
        CancellationToken cancellationToken = default)
    {
        var orderId = input.OrderId.Trim();
        var pago = await context.Pagos.Include(pago => pago.Inscripcion).ThenInclude(inscripcion => inscripcion.Curso)
            .SingleOrDefaultAsync(pago => pago.PayPalOrderId == orderId, cancellationToken)
            ?? throw new KeyNotFoundException("La orden de pago no existe.");
        if (!string.Equals(pago.Inscripcion.UserId, userId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("No puedes capturar una orden de otro usuario.");
        if (string.Equals(pago.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase))
            return new CapturarOrdenPagoResponse(pago.Id, pago.Inscripcion.CursoId, pago.Estado, pago.PayPalCaptureId);

        var captura = await payPal.CapturarOrdenAsync(orderId, cancellationToken);
        var referencia = $"CURSO-{pago.Inscripcion.CursoId}-USUARIO-{userId}";
        if (!string.Equals(captura.OrderId, orderId, StringComparison.Ordinal) ||
            !string.Equals(captura.Estado, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(captura.EstadoCaptura, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(captura.CaptureId) || captura.Monto != pago.Monto ||
            !string.Equals(captura.Moneda, pago.Moneda, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(captura.Referencia, referencia, StringComparison.Ordinal))
            throw new PayPalException("La captura de PayPal no pudo ser verificada.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        pago.PayPalCaptureId = captura.CaptureId;
        pago.Estado = "PAGADO";
        pago.FechaPago = DateTime.UtcNow;
        pago.Inscripcion.Estado = "ACTIVA";
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await EnviarConfirmacionAsync(userId, pago, cancellationToken);
        return new CapturarOrdenPagoResponse(pago.Id, pago.Inscripcion.CursoId, pago.Estado, pago.PayPalCaptureId);
    }

    public async Task<IReadOnlyList<PagoDetalleResponse>> MisComprasAsync(string userId,
        CancellationToken cancellationToken = default)
    {
        var pagos = await context.Pagos.AsNoTracking().Include(pago => pago.Inscripcion)
            .ThenInclude(inscripcion => inscripcion.Curso)
            .Where(pago => pago.Inscripcion.UserId == userId)
            .OrderByDescending(pago => pago.Id).ToListAsync(cancellationToken);
        return pagos.Select(Map).ToArray();
    }

    public async Task<PagoDetalleResponse?> ObtenerCompraAsync(string userId, int pagoId,
        CancellationToken cancellationToken = default)
    {
        var pago = await context.Pagos.AsNoTracking().Include(pago => pago.Inscripcion)
            .ThenInclude(inscripcion => inscripcion.Curso)
            .SingleOrDefaultAsync(pago => pago.Id == pagoId && pago.Inscripcion.UserId == userId, cancellationToken);
        return pago is null ? null : Map(pago);
    }

    private async Task EnviarConfirmacionAsync(string userId, Pago pago, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId);
        if (string.IsNullOrWhiteSpace(user?.Email)) return;
        try
        {
            await correoService.EnviarAsync(user.Email, "Pago confirmado",
                $"Tu pago de {pago.Monto:N2} {pago.Moneda} para el curso {pago.Inscripcion.Curso.Titulo} fue confirmado.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo enviar el correo de pago a {Destino}.", user.Email);
        }
    }

    private static PagoDetalleResponse Map(Pago pago) => new(pago.Id, pago.Inscripcion.CursoId,
        pago.Inscripcion.Curso.Titulo, pago.Monto, pago.Moneda, pago.Estado, pago.PayPalOrderId,
        pago.PayPalCaptureId, pago.FechaPago);
}
