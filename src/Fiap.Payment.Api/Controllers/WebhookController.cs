using Fiap.Payment.Api.SwaggerExamples.Common;
using Fiap.Payment.Api.SwaggerExamples.Webhook;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Webhook.Models.Request;
using Fiap.Payment.Application.Webhook.Models.Response;
using Fiap.Payment.Application.Webhook.Services;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

namespace Fiap.Payment.Api.Controllers;

/// <summary>
/// Controller responsible for processing webhook events.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class WebhookController(
    INotification notification,
    IWebhookService webhookService,
    ILogger<WebhookController> logger
    ) : BaseController(notification)
{
    /// <summary>
    /// Processes Stripe webhook events for payment notifications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns the processing result of the webhook event</returns>
    [HttpPost("stripe")]
    [SwaggerOperation(
        Summary = "Processes Stripe webhook events",
        Description = "Processes Stripe webhook events for payment notifications and updates the system accordingly."
    )]
    [ProducesResponseType(typeof(BaseResponse<WebhookResponse>), StatusCodes.Status200OK)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(WebhookResponseExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken = default)
    {
        Request.EnableBuffering();
            
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync(cancellationToken);
            
        Request.Body.Position = 0;
            
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            logger.LogWarning("Missing Stripe-Signature header");
            return BadRequest("Missing Stripe-Signature header");
        }

        var request = new WebhookRequest
        {
            Json = json,
            Signature = signature
        };

        var response = await webhookService.HandleStripeWebhookAsync(request, cancellationToken);
            
        return Response(BaseResponse<WebhookResponse>.Ok(response));
    }
}