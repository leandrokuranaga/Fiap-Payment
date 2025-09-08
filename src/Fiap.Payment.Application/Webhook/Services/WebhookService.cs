using System.Text.Json;
using Fiap.Contracts.Events;
using Fiap.Payment.Application.Common;
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
        ILogger<WebhookService> logger
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

            stripeEvent = EventUtility.ConstructEvent(request.Json, request.Signature, webhookSecret);

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    
                if (paymentIntent?.Metadata != null &&
                    paymentIntent.Metadata.TryGetValue("userId", out var userIdStr) &&
                    paymentIntent.Metadata.TryGetValue("gameId", out var gameIdStr) &&
                    int.TryParse(userIdStr, out var userId) &&
                    int.TryParse(gameIdStr, out var gameId))
                {
                    var payments = await paymentRepository.GetAsync(p => p.UserId == userId && p.GameId == gameId);
                    var payment = payments?.FirstOrDefault();
                        
                    if (payment != null)
                    {
                        var addGameEvent = new AddGameToUserLibraryEvent
                        {
                            GameId = gameId,
                            UserId = userId,
                            PurchaseDate = DateTime.UtcNow,
                            PricePaid = payment.Price
                        };

                        var contract = new AddGameToUserLibraryIntegrationEvent(
                            addGameEvent.GameId,
                            addGameEvent.UserId,
                            addGameEvent.PurchaseDate,
                            addGameEvent.PricePaid.Value,
                            addGameEvent.PricePaid.Currency
                        );

                        var eventJson = JsonSerializer.Serialize(contract);
                        var outbox = new OutboxMessage(typeof(AddGameToUserLibraryIntegrationEvent).AssemblyQualifiedName!, eventJson, DateTime.UtcNow);
                            
                        await outboxRepository.InsertOrUpdateAsync(outbox);
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
            else
            {
                logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                response.Success = true;
                response.Message = "Event type not handled";
            }         
            return response;
        });
    }
}
