namespace Fiap.Payment.Infra.CrossCutting.Http.Stripe.Models;

public record PurchaseGameRequest
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public string GameName { get; set; }
    public string UserEmail { get; set; }
    public decimal Price { get; set; }
}