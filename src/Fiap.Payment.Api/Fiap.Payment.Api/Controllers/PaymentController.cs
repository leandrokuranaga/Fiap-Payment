using Fiap.Payment.Api.SwaggerExamples.Common;
using Fiap.Payment.Api.SwaggerExamples.Payment;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;
using Fiap.Payment.Application.Payment.Services;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.Payment.Api.Controllers
{
    /// <summary>
    /// Controller responsible for payment operations.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PaymentController(INotification notification, IPaymentService paymentService) : BaseController(notification)
    {
        /// <summary>
        /// Method to process a payment for a game purchase.
        /// </summary>
        /// <param name="request">The information containing the user and game to be purchased</param>
        /// <returns>Returns the game purchased to be added to the library</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Method to process a payment for a game purchase.",
            Description = "Method to process a payment for a game purchase."
        )]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            var response = await paymentService.GamePurchase(request);
            return Response(BaseResponse<PaymentResponse>.Ok(response));
        }
    }  
}
