using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

namespace Fiap.Payment.Infra.CrossCutting.Http.Stripe
{
    public interface IStripeService
    {
        Task<Session> CreateCheckoutSessionAsync(PurchaseGameRequest request);
        Task<bool> VerifyWebhookAsync(string payload, string signature);
        Task ProcessSuccessfulPaymentAsync(string sessionId);
    }
}
