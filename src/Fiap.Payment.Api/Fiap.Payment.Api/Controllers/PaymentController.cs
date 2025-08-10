using System.Net;
using Fiap.Payment.Api.SwaggerExamples.Common;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Payment.Models.Request;
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
    public class PaymentController(INotification notification) : BaseController(notification)
    {
        /// <summary>
        /// Authenticates a user and generates a JWT token upon successful login.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>Returns a JWT token if authentication is successful; otherwise, returns an error.</returns>
        [HttpPost("payment")]
        [SwaggerOperation(
            Summary = "Authenticates a user and generates a JWT token upon successful login.",
            Description = "This endpoint verifies the user's credentials. If valid, it returns a signed JWT token that can be used to authorize further requests. " +
                          "If the credentials are invalid, a 400 or 404 error is returned depending on the nature of the failure."
        )]
        // [ProducesResponseType(typeof(SuccessResponse<LoginResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
        public IActionResult ProcessPayment([FromBody] PaymentRequest request)
        {
            return null;
            //if (request == null || string.IsNullOrEmpty(request.CardNumber))
            //{
            //    notification.AddNotification("Invalid payment request", ENotificationType.BadRequestError);
            //    return Response<PaymentResponse>(null, null);
            //}
            //var response = new PaymentResponse
            //{
            //    Success = true,
            //    TransactionId = Guid.NewGuid().ToString(),
            //    Message = "Payment processed successfully"
            //};
            //return Response(response);
        }
    }

  
}
