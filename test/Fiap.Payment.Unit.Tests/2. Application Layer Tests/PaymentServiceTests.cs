using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;
using Fiap.Payment.Application.Payment.Services;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.SeedWork;
using Moq;

namespace Fiap.Payment.Unit.Tests._2._Application_Layer_Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<INotification> _mockNotification;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockNotification = new Mock<INotification>();

            _paymentService = new PaymentService(
                _mockNotification.Object,
                _mockPaymentRepository.Object,
                null!, // stripeService - simplified for unit testing
                null!, // fCGService - simplified for unit testing
                null!  // cRMService - simplified for unit testing
            );
        }

        [Fact]
        public void PaymentService_ShouldBeCreated_WhenValidDependencies()
        {
            // Act & Assert
            Assert.NotNull(_paymentService);
        }

        [Fact]
        public void PaymentService_ShouldInheritFromBaseService()
        {
            // Act & Assert
            Assert.IsAssignableFrom<Fiap.Payment.Application.Common.BaseService>(_paymentService);
            Assert.IsAssignableFrom<IPaymentService>(_paymentService);
        }

        [Fact]
        public async Task GamePurchase_ShouldReturnResponse_WhenServiceCalled()
        {
            // Arrange
            var request = new PaymentRequest
            {
                UserId = 1,
                GameId = 6
            };

            // Act
            var result = await _paymentService.GamePurchase(request);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaymentResponse>(result);
        }
    }
}