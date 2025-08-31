using Fiap.Payment.Application.Payment.Models.Request;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Payment.Api.SwaggerExamples.Payment
{
    [ExcludeFromCodeCoverage]
    public class PaymentRequestExample : IExamplesProvider<PaymentRequest>
    {
        public PaymentRequest GetExamples()
        {
            return new PaymentRequest
            {
                UserId = 1,
                GameId = 6,
                GameName = "Minecraft",
                UserEmail = "le.s.kuranaga@gmail.com",
                Price = 26.95M
            };
        }
    }

}
