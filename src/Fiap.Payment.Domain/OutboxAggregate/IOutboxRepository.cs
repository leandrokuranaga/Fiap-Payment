using Fiap.Payment.Domain.SeedWork;

namespace Fiap.Payment.Domain.OutboxAggregate;

public interface IOutboxRepository : IBaseRepository<OutboxMessage>, IUnitOfWork
{
}