namespace Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models
{
    public record PaymentResult
    {
        public string Message { get; set; }
        public string PaymentUrl { get; set; }
        public bool Success { get; set; }
    }
}
