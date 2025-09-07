using Fiap.Payment.Api.Controllers;
using Fiap.Payment.Application.Common;
using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;
using Fiap.Payment.Application.Payment.Services;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Fiap.Payment.Unit.Tests._1._Api_Layer_Tests
{
    public class PaymentControllerTests
    {
        readonly IPaymentService _paymentServiceMock;
        readonly INotification _notificationMock;
        readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _paymentServiceMock = Substitute.For<IPaymentService>();
            _notificationMock = Substitute.For<INotification>();
            _controller = new PaymentController(_notificationMock, _paymentServiceMock)
            {
                ControllerContext = new ControllerContext
                {
                    RouteData = new Microsoft.AspNetCore.Routing.RouteData()
                }
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Payment";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";
        }

        #region ProcessPayment

        [Fact]
        public async Task ProcessPayment_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var request = new PaymentRequest
            {
                UserId = 1,
                GameId = 6
            };

            var response = new PaymentResponse
            {
                Message = "Payment processed successfully",
                PaymentUrl = "https://checkout.stripe.com/pay/test123"
            };

            _paymentServiceMock.GamePurchase(request).Returns(response);
            _notificationMock.HasNotification.Returns(false);

            // Act
            var result = await _controller.ProcessPayment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<PaymentResponse>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Equal(response.Message, baseResponse.Data.Message);
            Assert.Equal(response.PaymentUrl, baseResponse.Data.PaymentUrl);
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnBadRequest_WhenNotificationHasErrors()
        {
            // Arrange
            var request = new PaymentRequest
            {
                UserId = 1,
                GameId = 6
            };

            var response = new PaymentResponse
            {
                Message = "Game not found",
                PaymentUrl = string.Empty
            };

            var notificationModel = new NotificationModel
            {
                NotificationType = NotificationModel.ENotificationType.NotFound
            };

            _paymentServiceMock.GamePurchase(request).Returns(response);

            _notificationMock.HasNotification.Returns(true);
            _notificationMock.NotificationModel.Returns(notificationModel);

            // Act
            var result = await _controller.ProcessPayment(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<PaymentResponse>>(notFoundResult.Value);
            Assert.False(baseResponse.Success);
            Assert.NotNull(baseResponse.Error);
            Assert.Equal(NotificationModel.ENotificationType.NotFound, baseResponse.Error.NotificationType);
        }

        [Fact]
        public async Task ProcessPayment_ShouldCallService_WithCorrectParameters()
        {
            // Arrange
            var request = new PaymentRequest
            {
                UserId = 123,
                GameId = 456
            };

            var response = new PaymentResponse
            {
                Message = "Success",
                PaymentUrl = "https://test.com"
            };

            _paymentServiceMock.GamePurchase(Arg.Any<PaymentRequest>()).Returns(response);
            _notificationMock.HasNotification.Returns(false);

            // Act
            await _controller.ProcessPayment(request);

            // Assert
            await _paymentServiceMock.Received(1).GamePurchase(Arg.Is<PaymentRequest>(r => 
                r.UserId == request.UserId && 
                r.GameId == request.GameId));
        }

        #endregion
    }
}