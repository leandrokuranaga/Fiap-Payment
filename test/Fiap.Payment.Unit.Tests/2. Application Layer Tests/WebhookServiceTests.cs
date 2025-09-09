using Fiap.Payment.Application.Webhook.Models.Request;
using Fiap.Payment.Application.Webhook.Services;
using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using Stripe;
using Fiap.Payment.Application.Validators.WebhookValidators;

namespace Fiap.Payment.Unit.Tests._2._Application_Layer_Tests
{
    public class WebhookServiceTests
    {
        private readonly INotification _mockNotification;
        private readonly IOutboxRepository _mockOutboxRepository;
        private readonly IPaymentRepository _mockPaymentRepository;
        private readonly IConfiguration _mockConfiguration;
        private readonly ILogger<WebhookService> _mockLogger;
        private readonly IWebhookValidator _mockWebhookValidator;
        private readonly WebhookService _webhookService;

        public WebhookServiceTests()
        {
            _mockNotification = Substitute.For<INotification>();
            _mockOutboxRepository = Substitute.For<IOutboxRepository>();
            _mockPaymentRepository = Substitute.For<IPaymentRepository>();
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockLogger = Substitute.For<ILogger<WebhookService>>();
            _mockWebhookValidator = Substitute.For<IWebhookValidator>();

            _webhookService = new WebhookService(
                _mockNotification,
                _mockOutboxRepository,
                _mockPaymentRepository,
                _mockConfiguration,
                _mockLogger,
                _mockWebhookValidator
            );
        }

        [Fact]
        public async Task HandleStripeWebhookAsync_ShouldReturnError_WhenWebhookSecretNotConfigured()
        {
            // Arrange
            var request = new WebhookRequest
            {
                Json = "{\"type\":\"payment_intent.succeeded\"}",
                Signature = "test-signature"
            };

            _mockConfiguration["Stripe:WebhookSecret"].Returns((string)null!);

            // Act
            var result = await _webhookService.HandleStripeWebhookAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);

            _mockNotification.Received(1).AddNotification("WebhookSecret", "Webhook secret not configured.", NotificationModel.ENotificationType.BadRequestError);
        }

        [Fact]
        public async Task HandleStripeWebhookAsync_ShouldReturnSuccess_WhenEventProcessedSuccessfully()
        {
            // Arrange
            var validWebhookJson = @"{
                ""type"": ""payment_intent.succeeded"",
                ""data"": {
                    ""object"": {
                        ""metadata"": {
                            ""userId"": ""1"",
                            ""gameId"": ""6""
                        }
                    }
                }
            }";

            var request = new WebhookRequest
            {
                Json = validWebhookJson,
                Signature = "t=1234567890,v1=validSignature"
            };

            var payment = Domain.PaymentAggregate.Payment.Create(1, 6, 26.95m);
            var payments = new List<Domain.PaymentAggregate.Payment> { payment };

            _mockConfiguration["Stripe:WebhookSecret"].Returns("test_webhook_secret");

            // Create a mock Stripe event
            var mockPaymentIntent = new PaymentIntent();
            mockPaymentIntent.Metadata = new Dictionary<string, string>
            {
                ["userId"] = "1",
                ["gameId"] = "6"
            };

            var mockEvent = new Event
            {
                Type = "payment_intent.succeeded",
                Data = new EventData { Object = mockPaymentIntent }
            };

            _mockWebhookValidator.ConstructEvent(validWebhookJson, "t=1234567890,v1=validSignature", "test_webhook_secret")
                .Returns(mockEvent);

            _mockPaymentRepository.GetAsync(Arg.Any<Expression<Func<Domain.PaymentAggregate.Payment, bool>>>())
                .Returns(payments);

            _mockOutboxRepository.InsertOrUpdateAsync(Arg.Any<OutboxMessage>())
                .Returns(callInfo => callInfo.Arg<OutboxMessage>());

            _mockOutboxRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            // Act
            var result = await _webhookService.HandleStripeWebhookAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Webhook processed successfully", result.Message);

            _mockWebhookValidator.Received(1).ConstructEvent(validWebhookJson, "t=1234567890,v1=validSignature", "test_webhook_secret");
            await _mockPaymentRepository.Received(1).GetAsync(Arg.Any<Expression<Func<Domain.PaymentAggregate.Payment, bool>>>());
            await _mockOutboxRepository.Received(1).InsertOrUpdateAsync(Arg.Any<OutboxMessage>());
            await _mockOutboxRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task HandleStripeWebhookAsync_ShouldReturnSuccess_WhenEventTypeNotHandled()
        {
            // Arrange
            var unhandledEventJson = @"{
                ""type"": ""customer.created"",
                ""data"": {
                    ""object"": {}
                }
            }";

            var request = new WebhookRequest
            {
                Json = unhandledEventJson,
                Signature = "t=1234567890,v1=validSignature"
            };

            _mockConfiguration["Stripe:WebhookSecret"].Returns("test_webhook_secret");

            // Create a mock Stripe event for unhandled type
            var mockEvent = new Event
            {
                Type = "customer.created",
                Data = new EventData { Object = new Customer() }
            };

            _mockWebhookValidator.ConstructEvent(unhandledEventJson, "t=1234567890,v1=validSignature", "test_webhook_secret")
                .Returns(mockEvent);

            // Act
            var result = await _webhookService.HandleStripeWebhookAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Event type not handled", result.Message);

            _mockWebhookValidator.Received(1).ConstructEvent(unhandledEventJson, "t=1234567890,v1=validSignature", "test_webhook_secret");
            await _mockPaymentRepository.DidNotReceive().GetAsync(Arg.Any<Expression<Func<Domain.PaymentAggregate.Payment, bool>>>());
            await _mockOutboxRepository.DidNotReceive().InsertOrUpdateAsync(Arg.Any<OutboxMessage>());
        }
    }
}