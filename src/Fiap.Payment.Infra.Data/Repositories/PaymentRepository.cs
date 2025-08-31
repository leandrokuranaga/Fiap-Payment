using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Infra.Data.Repositories.Base;

namespace Fiap.Payment.Infra.Data.Repositories
{
    public class PaymentRepository(Context context) : BaseRepository<Domain.PaymentAggregate.Payment>(context), IPaymentRepository
    {
    }
}
