using Fiap.Payment.Common.Http.CRM.Models;
using Refit;

namespace Fiap.Payment.Common.Http.CRM
{
    public interface ICRMService
    {
        [Get("/api/v1/users/{id}")]
        Task<ApiEnvelope<UserResponse>> GetUser(int id);
    }
}
