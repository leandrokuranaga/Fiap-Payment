using Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

namespace Fiap.Payment.Application.Payment.Models.Request;

public record PaymentRequest
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    public string GameName { get; set; }
    public string UserEmail { get; set; }
    public decimal Price { get; set; }

    public static explicit operator PurchaseGameRequest(PaymentRequest payment)
    {
        return new PurchaseGameRequest
        {
            UserId = payment.UserId,
            GameId = payment.GameId,
            GameName = payment.GameName,
            UserEmail = payment.UserEmail,
            Price = payment.Price
        };
    }
}