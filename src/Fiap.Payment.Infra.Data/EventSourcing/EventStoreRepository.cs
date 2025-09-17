using EventStore.Client;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace Fiap.Payment.Infra.Data.EventSourcing
{
	public class EventStoreRepository(EventStoreClient eventStore, ILogger<EventStoreRepository> logger) : IEventStoreRepository
	{
		private static readonly Dictionary<string, Type> _eventTypes = AppDomain.CurrentDomain
			  .GetAssemblies()
			  .SelectMany(a => a.GetTypes())
			  .Where(t => t.IsSubclassOf(typeof(EventStored)))
			  .ToDictionary(t => t.Name, t => t);
		public async Task SaveAsync<T>(T @event) where T : EventStored
		{
			var eventData = new EventData(
				Uuid.NewUuid(),
				@event.GetType().Name,
				System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(@event)
			);

			await eventStore.AppendToStreamAsync(
				@event.StreamName,
				StreamState.Any,
				new[] { eventData }
			);
		}

		public async Task<IEnumerable<EventStored>> GetEventsAsync(string aggregate, string streamName)
		{
			var events = new List<EventStored>();

			var result = eventStore.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);

			await foreach (var resolvedEvent in result)
			{
				try
				{
					var eventTypeName = resolvedEvent.Event.EventType;
					if (!_eventTypes.TryGetValue(eventTypeName, out var eventType))
					{
						logger.LogWarning("Tipo de evento '{EventTypeName}' não encontrado nos assemblies carregados.", eventTypeName);
						continue;
					}

					var storedEvent = (EventStored)System.Text.Json.JsonSerializer.Deserialize(resolvedEvent.Event.Data.Span, eventType);
					if (storedEvent != null)
						events.Add(storedEvent);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Erro ao desserializar evento do stream {StreamName}", streamName);
					continue;
				}
			}

			return events;
		}

	}
}
