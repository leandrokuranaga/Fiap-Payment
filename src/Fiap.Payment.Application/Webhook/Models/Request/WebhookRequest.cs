namespace Fiap.Payment.Application.Webhook.Models.Request;

public record WebhookRequest
{
    public string Json { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}