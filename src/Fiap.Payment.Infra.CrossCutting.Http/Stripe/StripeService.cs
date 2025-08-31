using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Fiap.Payment.Infra.CrossCutting.Http.Stripe
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["Stripe:ApiKey"];

            StripeConfiguration.ApiKey = _apiKey;
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
                Success = true,
                PaymentUrl = link.Url,
                Message = "Link de pagamento criado com sucesso"
            };
        }


        // Aqui você adiciona a lógica para:
        // 1. Adicionar o jogo à biblioteca do usuário
        // 2. Atualizar o banco de dados
        // 3. Enviar email de confirmação
        // 4. Gerar chave do jogo, etc.

        // TODO: Implementar sua lógica de negócio aqui



    }
}
