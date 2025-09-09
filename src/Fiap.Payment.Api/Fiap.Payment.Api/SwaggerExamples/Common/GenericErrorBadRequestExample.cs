using System.Diagnostics.CodeAnalysis;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Domain.SeedWork;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.Payment.Api.SwaggerExamples.Common;

[ExcludeFromCodeCoverage]
public class GenericErrorBadRequestExample : IExamplesProvider<BaseResponse<object>>
{
    public BaseResponse<object> GetExamples()
    {
        var notification = new NotificationModel
        {
            NotificationType = NotificationModel.ENotificationType.BadRequestError
        };

        notification.AddMessage("Field", "Field Required");

        notification.AddMessage("Error", "Generic validation error");

        return BaseResponse<object>.Fail(notification);
    }
}