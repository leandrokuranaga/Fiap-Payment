namespace Fiap.Payment.Application.Webhook.Models.Response;

public record WebhookResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}