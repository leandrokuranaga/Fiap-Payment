using Abp.Domain.Uow;
using Fiap.Contracts.Events;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;
using Fiap.Payment.Common.Http.CRM;
using Fiap.Payment.Common.Http.FCG;
using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.SeedWork;
using Fiap.Payment.Infra.CrossCutting.Http.Stripe;
using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;
using System.Text.Json;

namespace Fiap.Payment.Application.Payment.Services
{
    public class PaymentService(
        INotification notification,
        IPaymentRepository paymentRepository,
        IStripeService stripeService,
        IFCGService fCGService,
        ICRMService cRMService,
        Domain.SeedWork.IUnitOfWork uow,
        IOutboxRepository outboxRepository
        ) : BaseService(notification), IPaymentService
    {
        public Task<PaymentResponse> GamePurchase(PaymentRequest request)
            => ExecuteAsync(async () =>
        {
            var response = new PaymentResponse();

            var user = await cRMService.GetUser(request.UserId);

            if (user is null)
            {
                notification.AddNotification("UserId", "User not Found.", NotificationModel.ENotificationType.NotFound);
                return response;
            }

            if (user?.Data?.GameIds?.Contains(request.GameId) is true)
            {
                notification.AddNotification("GameId", "User already has the game.", NotificationModel.ENotificationType.BusinessRules);
                return response;
            }

            var game = await fCGService.GetGame(request.GameId);

            if (game is null)
            {
                notification.AddNotification("GameId", "Game not found.", NotificationModel.ENotificationType.NotFound);
                return response;
            }

            var payment = Domain.PaymentAggregate.Payment.Create(request.UserId, request.GameId, game.Data.PriceWithDiscount);


            var paymentRequest = (PurchaseGameRequest)request;

            paymentRequest.Id = payment.Id;
            paymentRequest.GameName = game.Data.Name;
            paymentRequest.UserEmail = user.Data.Email;
            paymentRequest.Price = game.Data.PriceWithDiscount;

            await uow.BeginTransactionAsync();
            await paymentRepository.InsertOrUpdateAsync(payment);
            await paymentRepository.SaveChangesAsync();

            var result = await stripeService.PurchaseGame(paymentRequest);

            var paymentCreatedEvent = new PaymentCreatedIntegrationEvent(
                payment.Id,
                payment.UserId,
                payment.GameId,
                payment.Price.Value
            );

            var outbox = CreateOutboxMessage(paymentCreatedEvent);

            await outboxRepository.InsertOrUpdateAsync(outbox);
            await outboxRepository.SaveChangesAsync();

            await uow.CommitAsync();

            return (PaymentResponse)result;
        }
        );

        private OutboxMessage CreateOutboxMessage<T>(T content) where T : class =>
            new(typeof(T).AssemblyQualifiedName!, JsonSerializer.Serialize(content), DateTime.UtcNow);

    }
}
