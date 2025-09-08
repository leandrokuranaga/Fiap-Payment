using Stripe;

namespace Fiap.Payment.Application.Webhook.Services
{
    public class StripeWebhookValidator : IWebhookValidator
    {
        public Event ConstructEvent(string json, string signature, string secret)
        {
            return EventUtility.ConstructEvent(json, signature, secret);
        }
    }
}