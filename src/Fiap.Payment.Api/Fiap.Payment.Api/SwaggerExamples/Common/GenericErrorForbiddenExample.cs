using System.Diagnostics.CodeAnalysis;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Domain.SeedWork;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.Payment.Api.SwaggerExamples.Common;

[ExcludeFromCodeCoverage]
public class GenericErrorForbiddenExample : IExamplesProvider<BaseResponse<object>>
{
    public BaseResponse<object> GetExamples()
    {
        var notification = new NotificationModel
        {
            NotificationType = NotificationModel.ENotificationType.Unauthorized
        };

        notification.AddMessage("Permission", "You do not have permission to access this resource.");
        notification.AddMessage("Access", "Access is denied for this role or user.");

        return BaseResponse<object>.Fail(notification);
    }
}