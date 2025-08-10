using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Infra.Data.Repositories.Base;

namespace Fiap.Payment.Infra.Data.Repositories;

public class OutboxRepository(Context context) : BaseRepository<OutboxMessage>(context), IOutboxRepository
{
}