using Fiap.Payment.Api.Controllers;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Webhook.Models.Request;
using Fiap.Payment.Application.Webhook.Models.Response;
using Fiap.Payment.Application.Webhook.Services;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;

namespace Fiap.Payment.Unit.Tests._1._Api_Layer_Tests
{
    public class WebhookControllerTests
    {
        readonly IWebhookService _webhookServiceMock;
        readonly INotification _notificationMock;
        readonly ILogger<WebhookController> _loggerMock;
        readonly WebhookController _controller;

        public WebhookControllerTests()
        {
            _webhookServiceMock = Substitute.For<IWebhookService>();
            _notificationMock = Substitute.For<INotification>();
            _loggerMock = Substitute.For<ILogger<WebhookController>>();
            _controller = new WebhookController(_notificationMock, _webhookServiceMock, _loggerMock)
            {
                ControllerContext = new ControllerContext
                {
                    RouteData = new Microsoft.AspNetCore.Routing.RouteData()
                }
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Webhook";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";
        }

        #region StripeWebhook

        [Fact]
        public async Task StripeWebhook_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var json = "{\"type\":\"payment_intent.succeeded\",\"data\":{}}";
            var signature = "t=1234567890,v1=test_signature";

            var response = new WebhookResponse
            {
                Success = true,
                Message = "Webhook processed successfully"
            };

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            httpContext.Request.Headers["Stripe-Signature"] = signature;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData()
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Webhook";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";

            _webhookServiceMock
                .HandleStripeWebhookAsync(Arg.Any<WebhookRequest>(), Arg.Any<CancellationToken>())
                .Returns(response);

            _notificationMock.HasNotification.Returns(false);

            // Act
            var result = await _controller.StripeWebhook();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<WebhookResponse>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Equal(response.Message, baseResponse.Data.Message);
            Assert.True(baseResponse.Data.Success);
        }

        [Fact]
        public async Task StripeWebhook_ShouldReturnBadRequest_WhenSignatureIsMissing()
        {
            // Arrange
            var json = "{\"type\":\"payment_intent.succeeded\",\"data\":{}}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            // No Stripe-Signature header

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData()
            };

            // Act
            var result = await _controller.StripeWebhook();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing Stripe-Signature header", badRequestResult.Value);
        }

        [Fact]
        public async Task StripeWebhook_ShouldReturnBadRequest_WhenNotificationHasErrors()
        {
            // Arrange
            var json = "{\"type\":\"payment_intent.succeeded\",\"data\":{}}";
            var signature = "t=1234567890,v1=test_signature";

            var response = new WebhookResponse
            {
                Success = false,
                Message = "Invalid webhook signature"
            };

            var notificationModel = new NotificationModel
            {
                NotificationType = NotificationModel.ENotificationType.BadRequestError
            };

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            httpContext.Request.Headers["Stripe-Signature"] = signature;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData()
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Webhook";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";

            _webhookServiceMock
                .HandleStripeWebhookAsync(Arg.Any<WebhookRequest>(), Arg.Any<CancellationToken>())
                .Returns(response);

            _notificationMock.HasNotification.Returns(true);
            _notificationMock.NotificationModel.Returns(notificationModel);

            // Act
            var result = await _controller.StripeWebhook();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<WebhookResponse>>(badRequestResult.Value);
            Assert.False(baseResponse.Success);
            Assert.NotNull(baseResponse.Error);
            Assert.Equal(NotificationModel.ENotificationType.BadRequestError, baseResponse.Error.NotificationType);
        }

        [Fact]
        public async Task StripeWebhook_ShouldCallService_WithCorrectParameters()
        {
            // Arrange
            var json = "{\"type\":\"payment_intent.succeeded\",\"data\":{}}";
            var signature = "t=1234567890,v1=test_signature";

            var response = new WebhookResponse
            {
                Success = true,
                Message = "Success"
            };

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            httpContext.Request.Headers["Stripe-Signature"] = signature;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData()
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Webhook";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";

            _webhookServiceMock
                .HandleStripeWebhookAsync(Arg.Any<WebhookRequest>(), Arg.Any<CancellationToken>())
                .Returns(response);

            _notificationMock.HasNotification.Returns(false);

            // Act
            await _controller.StripeWebhook();

            // Assert
            await _webhookServiceMock.Received(1).HandleStripeWebhookAsync(Arg.Is<WebhookRequest>(r => 
                r.Json == json && 
                r.Signature == signature), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task StripeWebhook_ShouldLogWarning_WhenSignatureIsMissing()
        {
            // Arrange
            var json = "{\"type\":\"payment_intent.succeeded\",\"data\":{}}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            // No Stripe-Signature header

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData()
            };

            // Act
            await _controller.StripeWebhook();

            // Assert
            _loggerMock.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains("Missing Stripe-Signature header")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        #endregion
    }
}