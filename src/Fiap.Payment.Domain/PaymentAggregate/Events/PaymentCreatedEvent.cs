using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;
using Fiap.Payment.Domain.SeedWork;
using System.Text.Json;

namespace Fiap.Payment.Domain.PaymentAggregate.Events
{
	public record PaymentCreatedEvent : EventStored
	{
		public Guid PaymentId { get; set; }
		public int UserId { get; set; }
		public int GameId { get; set; }
		public Money Price { get; set; }
		public PaymentCreatedEvent() { }

		public PaymentCreatedEvent(Payment entity)
		{
			Id = Guid.NewGuid();
			PaymentId = entity.Id;
			UserId = entity.UserId;
			GameId = entity.GameId;
			Price = entity.Price;
			OccurredOn = DateTime.UtcNow;
			StreamName = $"{nameof(Payment)}-{entity.Id}";
			Type = nameof(Payment);
			Data = JsonSerializer.Serialize(entity);
		}
	}
}
