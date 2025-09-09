using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

namespace Fiap.Payment.Infra.CrossCutting.Http.Stripe
{
    public interface IStripeService
    {
        Task<PaymentResult> PurchaseGame(PurchaseGameRequest request, CancellationToken ct = default);
    }
}
