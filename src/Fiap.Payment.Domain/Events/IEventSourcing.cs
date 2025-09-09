namespace Fiap.Payment.Domain.Events
{
	public interface IEventSourcing
	{
		Guid Id { get; }
		DateTime OccurredOn { get; }
	}
}
