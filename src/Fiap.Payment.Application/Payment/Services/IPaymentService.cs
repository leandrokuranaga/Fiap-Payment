using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;

namespace Fiap.Payment.Application.Payment.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> GamePurchase(PaymentRequest request);
    }
}
