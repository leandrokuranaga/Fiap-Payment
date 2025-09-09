using Stripe;

namespace Fiap.Payment.Application.Validators.WebhookValidators
{
    public interface IWebhookValidator
    {
        Event ConstructEvent(string json, string signature, string secret);
    }
}