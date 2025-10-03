using Fiap.Payment.Domain.SeedWork;

namespace Fiap.Payment.Domain.PaymentAggregate
{
    public interface IPaymentRepository : IBaseRepository<Payment>, IUnitOfWork
    {
    }
}
