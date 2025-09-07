using Fiap.Payment.Common.Http.FCG.Models;
using Refit;

namespace Fiap.Payment.Common.Http.FCG
{
    public interface IFCGService
    {
        [Get("/api/v1/games/{id}")]
        Task<ApiEnvelope<GameResponse>> GetGame(int id);
    }
}
