using Fiap.Payment.Common.Http.CRM.Models;
using Refit;

namespace Fiap.Payment.Common.Http.CRM
{
    public interface ICRMService
    {
        [Get("/{id}")]
        Task<UserResponse> GetUser(int id);
    }
}
