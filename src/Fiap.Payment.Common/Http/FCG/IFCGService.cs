using Fiap.Payment.Common.Http.FCG.Models;
using Refit;

namespace Fiap.Payment.Common.Http.FCG
{
    public interface IFCGService
    {
        [Get("/{id}")]
        Task<GameResponse> GetGame(int id);
    }
}
