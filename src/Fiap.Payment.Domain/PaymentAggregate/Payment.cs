using Abp.Domain.Entities;
using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;

namespace Fiap.Payment.Domain.PaymentAggregate
{
    public sealed class Payment : Entity<Guid>
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public Money Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Payment()
        {            
        }

        public static Payment Create(int userId, int gameId, decimal price)
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = gameId,
                Price = new Money(price),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
