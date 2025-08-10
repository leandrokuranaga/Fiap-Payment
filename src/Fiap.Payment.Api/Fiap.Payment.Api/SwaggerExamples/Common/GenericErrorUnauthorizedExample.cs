using System.Diagnostics.CodeAnalysis;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Domain.SeedWork;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.Payment.Api.SwaggerExamples.Common;

[ExcludeFromCodeCoverage]
public class GenericErrorUnauthorizedExample : IExamplesProvider<BaseResponse<object>>
{
    public BaseResponse<object> GetExamples()
    {
        var notification = new NotificationModel
        {
            NotificationType = NotificationModel.ENotificationType.Unauthorized
        };

        notification.AddMessage("Authorization", "You are not authorized to perform this action.");
        notification.AddMessage("Token", "Missing or invalid authentication token.");

        return BaseResponse<object>.Fail(notification);
    }
}