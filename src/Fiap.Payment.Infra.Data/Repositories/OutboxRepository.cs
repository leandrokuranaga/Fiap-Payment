using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Infra.Data.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Fiap.Payment.Infra.Data.Repositories;

public class OutboxRepository(Context context, ILogger<UnitOfWork> logger) : BaseRepository<OutboxMessage>(context, logger), IOutboxRepository
{
}