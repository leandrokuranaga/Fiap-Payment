namespace Fiap.Payment.Domain.SeedWork
{
	public interface IEventStoreRepository
	{
		Task SaveAsync<T>(T @event) where T : EventStored;
		Task<IEnumerable<EventStored>> GetEventsAsync(string aggregate, string streamName);
	}
}
