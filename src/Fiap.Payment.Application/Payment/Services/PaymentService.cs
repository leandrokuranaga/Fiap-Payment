using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;
using Fiap.Payment.Common.Http.CRM;
using Fiap.Payment.Common.Http.FCG;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.SeedWork;
using Fiap.Payment.Infra.CrossCutting.Http.Stripe;
using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

namespace Fiap.Payment.Application.Payment.Services
{
    public class PaymentService(
        INotification notification,
        IPaymentRepository repository,
        IStripeService stripeService,
        IFCGService fCGService,
        ICRMService cRMService
        ) : BaseService(notification), IPaymentService
    {
        public Task<PaymentResponse> GamePurchase(PaymentRequest request) => ExecuteAsync(async () =>
        {
            var response = new PaymentResponse();

            var game = await fCGService.GetGame(request.GameId);

            if (game == null)
            {
                notification.AddNotification("GameId", "Game not found.", NotificationModel.ENotificationType.NotFound);
                return response;
            }

            var user = await cRMService.GetUser(request.UserId);

            if (user == null)
            {
                notification.AddNotification("UserId", "User not Found.", NotificationModel.ENotificationType.NotFound);
                return response;
            }

            var payment = Domain.PaymentAggregate.Payment.Create(request.UserId, request.GameId, game.PriceWithDiscount);

            await repository.InsertOrUpdateAsync(payment);
            await repository.SaveChangesAsync();

            var paymentRequest = (PurchaseGameRequest)request;

            paymentRequest.Id = payment.Id;

            var result = await stripeService.PurchaseGame(paymentRequest);

            return (PaymentResponse)result;
        });
    }
}
