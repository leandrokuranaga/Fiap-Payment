using Fiap.Payment.Application.Webhook.Models.Request;
using Fiap.Payment.Application.Webhook.Models.Response;
using Fiap.Payment.Application.Webhook.Services;
using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;
using Fiap.Payment.Domain.SeedWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace Fiap.Payment.Unit.Tests._2._Application_Layer_Tests
{
    public class WebhookServiceTests
    {
        private readonly Mock<INotification> _mockNotification;
        private readonly Mock<IOutboxRepository> _mockOutboxRepository;
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<WebhookService>> _mockLogger;
        private readonly WebhookService _webhookService;

        public WebhookServiceTests()
        {
            _mockNotification = new Mock<INotification>();
            _mockOutboxRepository = new Mock<IOutboxRepository>();
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<WebhookService>>();

            _webhookService = new WebhookService(
                _mockNotification.Object,
                _mockOutboxRepository.Object,
                _mockPaymentRepository.Object,
                _mockConfiguration.Object,
                _mockLogger.Object
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

            _mockConfiguration
                .Setup(x => x["Stripe:WebhookSecret"])
                .Returns((string)null!);

            // Act
            var result = await _webhookService.HandleStripeWebhookAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);

            _mockNotification.Verify(n =>
                n.AddNotification("WebhookSecret", "Webhook secret not configured.", NotificationModel.ENotificationType.BadRequestError),
                Times.Once);
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

            _mockConfiguration
                .Setup(x => x["Stripe:WebhookSecret"])
                .Returns("test_webhook_secret");

            _mockPaymentRepository
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<Domain.PaymentAggregate.Payment, bool>>>()))
                .ReturnsAsync(payments);

            _mockOutboxRepository
                .Setup(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()))
                .ReturnsAsync((OutboxMessage outbox) => outbox);

            _mockOutboxRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _webhookService.HandleStripeWebhookAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Webhook processed successfully", result.Message);

            _mockPaymentRepository.Verify(x => x.GetAsync(It.IsAny<Expression<Func<Domain.PaymentAggregate.Payment, bool>>>()), Times.Once);
            _mockOutboxRepository.Verify(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()), Times.Once);
            _mockOutboxRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
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

            _mockConfiguration
                .Setup(x => x["Stripe:WebhookSecret"])
                .Returns("test_webhook_secret");

            // Act
            var result = await _webhookService.HandleStripeWebhookAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Event type not handled", result.Message);

            _mockPaymentRepository.Verify(x => x.GetAsync(It.IsAny<Expression<Func<Domain.PaymentAggregate.Payment, bool>>>()), Times.Never);
            _mockOutboxRepository.Verify(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()), Times.Never);
        }
    }
}