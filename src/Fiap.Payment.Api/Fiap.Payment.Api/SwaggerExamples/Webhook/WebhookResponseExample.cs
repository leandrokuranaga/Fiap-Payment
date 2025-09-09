using Fiap.Payment.Application.Webhook.Models.Response;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Payment.Api.SwaggerExamples.Webhook
{
    [ExcludeFromCodeCoverage]
    public class WebhookResponseExample : IExamplesProvider<WebhookResponse>
    {
        public WebhookResponse GetExamples()
        {
            return new WebhookResponse
            {
                Success = true,
                Message = "Webhook processed successfully"
            };
        }
    }
}