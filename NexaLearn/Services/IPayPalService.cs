using NexaLearn.DTOs.Pagos;

namespace NexaLearn.Services;

public interface IPayPalService
{
    Task<string> ObtenerAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<PayPalOrdenResponse> CrearOrdenAsync(decimal monto, string moneda, string referencia,
        CancellationToken cancellationToken = default);
    Task<PayPalCapturaResponse> CapturarOrdenAsync(string orderId,
        CancellationToken cancellationToken = default);
    Task<PayPalOrdenResponse> ConsultarOrdenAsync(string orderId,
        CancellationToken cancellationToken = default);
}
