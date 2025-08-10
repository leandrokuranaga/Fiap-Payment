using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;
using Microsoft.Extensions.Configuration;

namespace Fiap.Payment.Infra.CrossCutting.Http.Stripe
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _webhookSecret;

        public StripeService(IConfiguration configuration, string webhookSecret)
        {
            _configuration = configuration;
            _webhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        public async Task<Session> CreateCheckoutSessionAsync(PurchaseGameRequest request)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "pix" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(request.Price * 100),
                        Currency = "brl",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.GameName,
                            Description = $"Compra do jogo: {request.GameName}",
                        },
                    },
                    Quantity = 1,
                },
            },
                Mode = "payment",
                SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _configuration["Stripe:CancelUrl"],
                Metadata = new Dictionary<string, string>
            {
                {"game_id", request.GameId},
                {"user_id", request.UserId}
            }
            };

            var service = new SessionService();
            return await service.CreateAsync(options);
        }

        public async Task<bool> VerifyWebhookAsync(string payload, string signature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);
                return true;
            }
            catch (StripeException)
            {
                return false;
            }
        }

        public async Task ProcessSuccessfulPaymentAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                var gameId = session.Metadata["game_id"];
                var userId = session.Metadata["user_id"];

                // Aqui você adiciona a lógica para:
                // 1. Adicionar o jogo à biblioteca do usuário
                // 2. Atualizar o banco de dados
                // 3. Enviar email de confirmação
                // 4. Gerar chave do jogo, etc.

                Console.WriteLine($"Pagamento confirmado - Jogo {gameId} comprado pelo usuário {userId}");
                // TODO: Implementar sua lógica de negócio aqui
            }
        }
    }
}
