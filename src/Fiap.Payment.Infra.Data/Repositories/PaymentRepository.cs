using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Infra.Data.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Fiap.Payment.Infra.Data.Repositories
{
    public class PaymentRepository(Context context, ILogger<UnitOfWork> logger) : BaseRepository<Domain.PaymentAggregate.Payment>(context, logger), IPaymentRepository
    {
    }
}
