using Fiap.Payment.Domain.Events;
using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;

namespace Fiap.Payment.Domain.PaymentAggregate.Events
{
	// TODO
	public record PaymentCreatedEvent : EventStored
	{
		public Guid PaymenteId { get; set; }
		public int UserId { get; set; }
		public int GameId { get; set; }
		public Money Price { get; set; }
		public PaymentCreatedEvent() { }

		public PaymentCreatedEvent(Payment entity)
		{
			Id = Guid.NewGuid();
			PaymenteId = entity.Id;
			UserId = entity.UserId;
			GameId = entity.GameId;
			Price = entity.Price;
			OccurredOn = DateTime.UtcNow;
			StreamName = $"{nameof(Payment)}-{entity.Id}";
			Type = nameof(Payment);
			Data = System.Text.Json.JsonSerializer.Serialize(entity);
		}
	}
}
