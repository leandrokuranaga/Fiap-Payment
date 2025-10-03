using System.Text.Json;
using Fiap.Contracts.Events;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Validators.WebhookValidators;
using Fiap.Payment.Application.Webhook.Models.Request;
using Fiap.Payment.Application.Webhook.Models.Response;
using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.PaymentAggregate.Events;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Fiap.Payment.Application.Webhook.Services
{
    public class WebhookService(
        INotification notification,
        IOutboxRepository outboxRepository,
        IPaymentRepository paymentRepository,
        IConfiguration configuration,
        ILogger<WebhookService> logger,
        IWebhookValidator webhookValidator
        ) : BaseService(notification), IWebhookService
    {
        public Task<WebhookResponse> HandleStripeWebhookAsync(WebhookRequest request, CancellationToken ct = default) => ExecuteAsync(async () =>
        {
            var response = new WebhookResponse();

            var webhookSecret = configuration["Stripe:WebhookSecret"];
                
            if (string.IsNullOrEmpty(webhookSecret))
            {
                notification.AddNotification("WebhookSecret", "Webhook secret not configured.", NotificationModel.ENotificationType.BadRequestError);
                return response;
            }

            Event stripeEvent;

            stripeEvent = webhookValidator.ConstructEvent(request.Json, request.Signature, webhookSecret);

            if (stripeEvent.Type is "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    
                if (paymentIntent?.Metadata is not null &&
                    paymentIntent.Metadata.TryGetValue("userId", out var userIdStr) &&
                    paymentIntent.Metadata.TryGetValue("gameId", out var gameIdStr) &&
                    int.TryParse(userIdStr, out var userId) &&
                    int.TryParse(gameIdStr, out var gameId))
                {
                    var payments = await paymentRepository.GetAsync(p => p.UserId == userId && p.GameId == gameId);
                    var payment = payments?.FirstOrDefault();
                        
                    if (payment is not null)
                    {
                        var addGameEvent = new AddGameToUserLibraryEvent
                        {
                            GameId = gameId,
                            UserId = userId,
                            PurchaseDate = DateTime.UtcNow,
                            PricePaid = payment.Price
                        };

                        var addGameContract = new AddGameToUserLibraryIntegrationEvent(
                            addGameEvent.GameId,
                            addGameEvent.UserId,
                            addGameEvent.PurchaseDate,
                            addGameEvent.PricePaid.Value,
                            addGameEvent.PricePaid.Currency
                        );

                        var paymentSuccessfulContract = new PaymentSuccessfulIntegrationEvent(
                            payment.Id,
                            payment.UserId,
                            payment.GameId,
                            payment.Price.Value
                        );

                        var addGameEventJson = JsonSerializer.Serialize(addGameContract);
                        var addGameOutbox = new OutboxMessage(typeof(AddGameToUserLibraryIntegrationEvent).AssemblyQualifiedName!, addGameEventJson, DateTime.UtcNow);

                        var paymentSuccessfulEventJson = JsonSerializer.Serialize(paymentSuccessfulContract);
                        var paymentSuccessfulOutbox = new OutboxMessage(typeof(PaymentSuccessfulIntegrationEvent).AssemblyQualifiedName!, paymentSuccessfulEventJson, DateTime.UtcNow);
                            
                        await outboxRepository.InsertOrUpdateAsync(addGameOutbox);
                        await outboxRepository.InsertOrUpdateAsync(paymentSuccessfulOutbox);
                        await outboxRepository.SaveChangesAsync();

                        logger.LogInformation("Payment successful for UserId: {UserId}, GameId: {GameId}", userId, gameId);
                            
                        response.Success = true;
                        response.Message = "Webhook processed successfully";
                    }
                    else
                    {
                        logger.LogWarning("Payment not found for UserId: {UserId}, GameId: {GameId}", userId, gameId);
                        notification.AddNotification("Payment", "Payment not found.", NotificationModel.ENotificationType.NotFound);
                    }
                }
                else
                {
                    logger.LogWarning("Missing or invalid metadata in payment intent");
                    notification.AddNotification("Metadata", "Missing or invalid payment metadata.", NotificationModel.ENotificationType.BadRequestError);
                }
            }
            else if (stripeEvent.Type is "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    
                if (paymentIntent?.Metadata is not null &&
                    paymentIntent.Metadata.TryGetValue("userId", out var userIdStr) &&
                    paymentIntent.Metadata.TryGetValue("gameId", out var gameIdStr) &&
                    int.TryParse(userIdStr, out var userId) &&
                    int.TryParse(gameIdStr, out var gameId))
                {
                    var payments = await paymentRepository.GetAsync(p => p.UserId == userId && p.GameId == gameId);
                    var payment = payments?.FirstOrDefault();
                        
                    if (payment is not null)
                    {
                        var paymentFailedContract = new PaymentFailedIntegrationEvent(
                            payment.Id,
                            payment.UserId,
                            payment.GameId,
                            payment.Price.Value
                        );

                        var paymentFailedEventJson = JsonSerializer.Serialize(paymentFailedContract);
                        var paymentFailedOutbox = new OutboxMessage(typeof(PaymentFailedIntegrationEvent).AssemblyQualifiedName!, paymentFailedEventJson, DateTime.UtcNow);
                            
                        await outboxRepository.InsertOrUpdateAsync(paymentFailedOutbox);
                        await outboxRepository.SaveChangesAsync();

                        logger.LogWarning("Payment failed for UserId: {UserId}, GameId: {GameId}", userId, gameId);
                            
                        response.Success = true;
                        response.Message = "Payment failed webhook processed successfully";
                    }
                    else
                    {
                        logger.LogWarning("Payment not found for failed payment - UserId: {UserId}, GameId: {GameId}", userId, gameId);
                        notification.AddNotification("Payment", "Payment not found for failed payment.", NotificationModel.ENotificationType.NotFound);
                    }
                }
                else
                {
                    logger.LogWarning("Missing or invalid metadata in failed payment intent");
                    notification.AddNotification("Metadata", "Missing or invalid payment metadata for failed payment.", NotificationModel.ENotificationType.BadRequestError);
                }
            }
            else
            {
                logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                response.Success = true;
                response.Message = "Event type not handled";
            }         
            return response;
        });

        private OutboxMessage CreateOutboxMessage<T>(T content) where T : class =>
            new(typeof(T).AssemblyQualifiedName!, JsonSerializer.Serialize(content), DateTime.UtcNow);
    }
}
