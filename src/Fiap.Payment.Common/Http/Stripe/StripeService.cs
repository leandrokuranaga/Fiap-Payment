using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Fiap.Payment.Infra.CrossCutting.Http.Stripe
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;
        private readonly string _apiKey;
        private readonly string _webhook;

        public StripeService(
            IConfiguration configuration,
            ILogger<StripeService> logger
        )
        {
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Stripe:ApiKey"];
            _webhook = _configuration["Stripe:WebhookSecret"];

            StripeConfiguration.ApiKey = _apiKey;
        }

        public async Task HandleWebhookAsync(string json, string signature, CancellationToken ct = default)
        {
            _logger.LogInformation("Processing Stripe webhook");
                
            var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhook);
                
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentSucceeded(stripeEvent, ct);
                    break;
                case "payment_intent.payment_failed":
                    await HandlePaymentFailed(stripeEvent, ct);
                    break;
                default:
                    _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }            
        }

        private async Task HandlePaymentSucceeded(Event stripeEvent, CancellationToken ct)
        {
            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
                return;

            _logger.LogInformation("Payment succeeded for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

            var metadata = paymentIntent.Metadata;
            if (!metadata.TryGetValue("paymentId", out var paymentId) ||
                !metadata.TryGetValue("userId", out var userId) ||
                !metadata.TryGetValue("gameId", out var gameId))
            {
                _logger.LogWarning("Required metadata missing from PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                return;
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent, CancellationToken ct)
        {
            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
                return;

            _logger.LogInformation("Payment failed for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            
            await Task.CompletedTask;
        }

        public async Task<PaymentResult> PurchaseGame(PurchaseGameRequest request, CancellationToken ct)
        {
            var priceSvc = new PriceService();
            var price = await priceSvc.CreateAsync(new PriceCreateOptions
            {
                UnitAmount = (long)(request.Price * 100),
                Currency = "brl",
                ProductData = new PriceProductDataOptions
                {
                    Name = $"Compra do jogo {request.GameName}"
                }
            }, cancellationToken: ct);

            var linkSvc = new PaymentLinkService();
            var link = await linkSvc.CreateAsync(new PaymentLinkCreateOptions
            {
                LineItems =
                [
                    new() { Price = price.Id, Quantity = 1 }
                ],
                PaymentIntentData = new PaymentLinkPaymentIntentDataOptions
                {
                Metadata = new Dictionary<string, string>
                {
                    { "paymentId", request.Id.ToString() },
                    { "userId", request.UserId.ToString() },
                    { "gameId", request.GameId.ToString() }
                }
                },
                AfterCompletion = new PaymentLinkAfterCompletionOptions
                {
                    Type = "hosted_confirmation",
                    HostedConfirmation = new PaymentLinkAfterCompletionHostedConfirmationOptions
                    {
                        CustomMessage = "Pagamento concluído! Obrigado pela compra."
                    }
                },
                BillingAddressCollection = "auto",
                PhoneNumberCollection = new PaymentLinkPhoneNumberCollectionOptions { Enabled = false },
                AllowPromotionCodes = false,
                CustomerCreation = "if_required"
            }, cancellationToken: ct);

            return new PaymentResult
            {
                PaymentUrl = link.Url,
                Message = "Link de pagamento criado com sucesso"
            };
        }
    }
}
