using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

namespace Fiap.Payment.Application.Payment.Models.Response
{
    public record PaymentResponse
    {
        public string Message { get; set; }
        public string PaymentUrl { get; set; }

        public static explicit operator PaymentResponse(PaymentResult payment)
        {
            return new PaymentResponse
            {
                Message = payment.Message,
                PaymentUrl = payment.PaymentUrl
            };
        }
    }
}
