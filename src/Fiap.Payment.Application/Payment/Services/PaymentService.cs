using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.SeedWork;
using Fiap.Payment.Infra.CrossCutting.Http.Stripe;
using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

namespace Fiap.Payment.Application.Payment.Services
{
    public class PaymentService(
        INotification notification,
        IPaymentRepository repository,
        IStripeService stripeService
        ) : IPaymentService
    {
        public async Task<PaymentResponse> GamePurchase(PaymentRequest request)
        {
            var payment = Domain.PaymentAggregate.Payment.Create(request.UserId, request.GameId, request.Price);

            await repository.InsertOrUpdateAsync(payment);

            var paymentRequest = (PurchaseGameRequest)request;

            var result = await stripeService.PurchaseGame(paymentRequest);

            return (PaymentResponse)result;
        }
    }
}
