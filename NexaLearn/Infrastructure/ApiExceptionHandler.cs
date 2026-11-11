using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexaLearn.Exceptions;

namespace NexaLearn.Infrastructure;

public sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, detail) = exception switch
        {
            ImagenInvalidaException or ValidationException => (400, exception.Message),
            ConflictoException => (409, exception.Message),
            DbUpdateConcurrencyException => (409, "El registro cambió o fue eliminado. Vuelve a consultarlo."),
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } }
                => (409, "Ya existe un registro con esos datos únicos."),
            DbUpdateException { InnerException: SqlException { Number: 547 } }
                => (409, "La operación entra en conflicto con registros relacionados."),
            _ => (0, string.Empty)
        };
        if (status == 0) return false;

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = status == 400 ? "Datos inválidos" : "Conflicto al guardar los datos",
                Detail = detail
            }
        });
    }
}
