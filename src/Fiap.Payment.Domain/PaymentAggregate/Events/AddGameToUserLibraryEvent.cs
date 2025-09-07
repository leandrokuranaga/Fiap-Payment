using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;

namespace Fiap.Payment.Domain.PaymentAggregate.Events
{
    public class AddGameToUserLibraryEvent
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public Money PricePaid { get; set; }
    }
}
