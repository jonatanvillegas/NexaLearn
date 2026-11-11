namespace NexaLearn.DTOs.Pagos;

public record PayPalOrdenResponse(string OrderId, string Estado, string? ApprovalUrl,
    string? Referencia, decimal Monto, string Moneda);
