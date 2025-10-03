namespace Fiap.Payment.Domain.SeedWork
{
	public interface IEventSourcing
	{
		Guid Id { get; }
		DateTime OccurredOn { get; }
	}
}
