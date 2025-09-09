using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

namespace Fiap.Payment.Application.Payment.Models.Request;

public record PaymentRequest
{
    public int UserId { get; set; }
    public int GameId { get; set; }

    public static explicit operator PurchaseGameRequest(PaymentRequest payment)
    {
        return new PurchaseGameRequest
        {
            UserId = payment.UserId,
            GameId = payment.GameId
        };
    }
}