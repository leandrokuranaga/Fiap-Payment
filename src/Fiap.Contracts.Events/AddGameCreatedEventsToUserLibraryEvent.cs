using Fiap.Payment.Domain.Events;

namespace Fiap.Contracts.Events;

public sealed record AddGameCreatedEventsToUserLibraryEvent : EventStored
{
	public Guid PaymenteId { get; set; }
	public int UserId { get; set; }
	public int GameId { get; set; }
	public decimal PricePaidValue { get; set; }
	public string? PricePaidCurrency { get; set; }

	public AddGameCreatedEventsToUserLibraryEvent() { }
	// TODO
	public AddGameCreatedEventsToUserLibraryEvent(AddGameToUserLibraryIntegrationEvent entity)
	{		
		UserId = entity.UserId;
		GameId = entity.GameId;	
		OccurredOn = DateTime.UtcNow;
		StreamName = $"{nameof(Payment)}-{entity.UserId}-{entity.GameId}";
		Type = nameof(AddGameToUserLibraryIntegrationEvent);
		Data = System.Text.Json.JsonSerializer.Serialize(entity);
	}
}
