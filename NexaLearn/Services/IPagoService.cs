using NexaLearn.DTOs.Pagos;

namespace NexaLearn.Services;

public interface IPagoService
{
    Task<CrearOrdenPagoResponse> CrearOrdenAsync(string userId, CrearOrdenPagoRequest input,
        CancellationToken cancellationToken = default);
    Task<CapturarOrdenPagoResponse> CapturarOrdenAsync(string userId, CapturarOrdenPagoRequest input,
        CancellationToken cancellationToken = default);
    Task<ReanudarOrdenPagoResponse> ReanudarOrdenAsync(string userId, CrearOrdenPagoRequest input,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PagoDetalleResponse>> MisComprasAsync(string userId,
        CancellationToken cancellationToken = default);
    Task<PagoDetalleResponse?> ObtenerCompraAsync(string userId, int pagoId,
        CancellationToken cancellationToken = default);
}
