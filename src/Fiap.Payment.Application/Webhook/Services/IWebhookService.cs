using Fiap.Payment.Application.Webhook.Models.Request;
using Fiap.Payment.Application.Webhook.Models.Response;

namespace Fiap.Payment.Application.Webhook.Services
{
    public interface IWebhookService
    {
        Task<WebhookResponse> HandleStripeWebhookAsync(WebhookRequest request, CancellationToken ct = default);
    }
}
