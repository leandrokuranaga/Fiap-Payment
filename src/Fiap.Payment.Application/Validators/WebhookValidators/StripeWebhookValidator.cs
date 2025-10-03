using Stripe;

namespace Fiap.Payment.Application.Validators.WebhookValidators
{
    public class StripeWebhookValidator : IWebhookValidator
    {
        public Event ConstructEvent(string json, string signature, string secret)
        {
            return EventUtility.ConstructEvent(json, signature, secret);
        }
    }
}