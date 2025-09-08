using Stripe;

namespace Fiap.Payment.Application.Webhook.Services
{
    public interface IWebhookValidator
    {
        Event ConstructEvent(string json, string signature, string secret);
    }
}