using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NexaLearn.DTOs.Pagos;
using NexaLearn.Exceptions;

namespace NexaLearn.Services;

public sealed class PayPalService(HttpClient client, IConfiguration configuration) : IPayPalService
{
    public async Task<string> ObtenerAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var clientId = configuration["PayPal:ClientId"];
        var clientSecret = configuration["PayPal:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new PayPalException("Configura PayPal:ClientId y PayPal:ClientSecret.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token")
        {
            Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("grant_type", "client_credentials")])
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

        using var response = await client.SendAsync(request, cancellationToken);
        var json = await EnsureSuccessAsync(response, cancellationToken);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("access_token").GetString()
            ?? throw new PayPalException("PayPal no devolvió un access token.");
    }

    public async Task<PayPalOrdenResponse> CrearOrdenAsync(decimal monto, string moneda, string referencia,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = referencia,
                    description = "Compra de curso NexaLearn",
                    amount = new { currency_code = moneda, value = monto.ToString("0.00", CultureInfo.InvariantCulture) }
                }
            },
            payment_source = new
            {
                paypal = new
                {
                    experience_context = new
                    {
                        return_url = configuration["PayPal:ReturnUrl"],
                        cancel_url = configuration["PayPal:CancelUrl"],
                        landing_page = "LOGIN",
                        user_action = "PAY_NOW"
                    }
                }
            }
        };

        var json = await EnviarAutorizadoAsync(HttpMethod.Post, "v2/checkout/orders", body,
            $"crear-{Guid.NewGuid():N}", cancellationToken);
        return LeerOrden(json);
    }

    public async Task<PayPalCapturaResponse> CapturarOrdenAsync(string orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await EnviarAutorizadoAsync(HttpMethod.Post,
                $"v2/checkout/orders/{Uri.EscapeDataString(orderId)}/capture", null,
                $"capturar-{orderId}", cancellationToken);
            return LeerCaptura(json);
        }
        catch (PayPalException)
        {
            // PayPal puede responder que ya fue capturada. Consultarla permite reintentar
            // la persistencia local sin volver a cobrar al estudiante.
            var orden = await ConsultarOrdenAsync(orderId, cancellationToken);
            if (string.Equals(orden.Estado, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                var json = await EnviarAutorizadoAsync(HttpMethod.Get,
                    $"v2/checkout/orders/{Uri.EscapeDataString(orderId)}", null, null, cancellationToken);
                return LeerCaptura(json);
            }
            throw;
        }
    }

    public async Task<PayPalOrdenResponse> ConsultarOrdenAsync(string orderId,
        CancellationToken cancellationToken = default)
    {
        var json = await EnviarAutorizadoAsync(HttpMethod.Get,
            $"v2/checkout/orders/{Uri.EscapeDataString(orderId)}", null, null, cancellationToken);
        return LeerOrden(json);
    }

    private async Task<string> EnviarAutorizadoAsync(HttpMethod method, string url, object? body,
        string? requestId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            await ObtenerAccessTokenAsync(cancellationToken));
        if (!string.IsNullOrWhiteSpace(requestId)) request.Headers.Add("PayPal-Request-Id", requestId);
        if (body is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        return await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<string> EnsureSuccessAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new PayPalException($"PayPal no pudo procesar la solicitud ({(int)response.StatusCode}).");
        return json;
    }

    private static PayPalOrdenResponse LeerOrden(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var unit = root.GetProperty("purchase_units")[0];
        var amount = unit.GetProperty("amount");
        var approvalUrl = root.TryGetProperty("links", out var links)
            ? links.EnumerateArray().FirstOrDefault(link =>
                link.GetProperty("rel").GetString() is "payer-action" or "approve")
                .GetPropertyOrNull("href")
            : null;

        return new PayPalOrdenResponse(root.GetProperty("id").GetString()!, root.GetProperty("status").GetString()!,
            approvalUrl, unit.GetProperty("reference_id").GetString(), LeerMonto(amount),
            amount.GetProperty("currency_code").GetString()!);
    }

    private static PayPalCapturaResponse LeerCaptura(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var unit = root.GetProperty("purchase_units")[0];
        var capture = unit.GetProperty("payments").GetProperty("captures")[0];
        var amount = capture.GetProperty("amount");
        return new PayPalCapturaResponse(root.GetProperty("id").GetString()!, root.GetProperty("status").GetString()!,
            capture.GetProperty("id").GetString(), capture.GetProperty("status").GetString(),
            unit.GetProperty("reference_id").GetString(), LeerMonto(amount), amount.GetProperty("currency_code").GetString()!);
    }

    private static decimal LeerMonto(JsonElement amount)
        => decimal.Parse(amount.GetProperty("value").GetString()!, CultureInfo.InvariantCulture);
}

file static class JsonElementExtensions
{
    public static string? GetPropertyOrNull(this JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Undefined || !element.TryGetProperty(property, out var value)
            ? null : value.GetString();
}
