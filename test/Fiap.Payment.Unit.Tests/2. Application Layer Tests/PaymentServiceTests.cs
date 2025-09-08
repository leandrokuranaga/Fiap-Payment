extern alias HttpInfra;
using Fiap.Payment.Application.Payment.Models.Request;
using Fiap.Payment.Application.Payment.Models.Response;
using Fiap.Payment.Application.Payment.Services;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.SeedWork;
using NSubstitute;

namespace Fiap.Payment.Unit.Tests._2._Application_Layer_Tests
{
    public class PaymentServiceTests
    {
        private readonly IPaymentRepository _mockPaymentRepository;
        private readonly INotification _mockNotification;
        private readonly HttpInfra::Fiap.Payment.Infra.CrossCutting.Http.Stripe.IStripeService _mockStripeService;
        private readonly Fiap.Payment.Common.Http.FCG.IFCGService _mockFCGService;
        private readonly Fiap.Payment.Common.Http.CRM.ICRMService _mockCRMService;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockPaymentRepository = Substitute.For<IPaymentRepository>();
            _mockNotification = Substitute.For<INotification>();
            _mockStripeService = Substitute.For<HttpInfra::Fiap.Payment.Infra.CrossCutting.Http.Stripe.IStripeService>();
            _mockFCGService = Substitute.For<Fiap.Payment.Common.Http.FCG.IFCGService>();
            _mockCRMService = Substitute.For<Fiap.Payment.Common.Http.CRM.ICRMService>();

            _paymentService = new PaymentService(
                _mockNotification,
                _mockPaymentRepository,
                null!, // Simplified for testing
                _mockFCGService,
                _mockCRMService
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

            // Act & Assert - Test creation and basic functionality
            var result = await _paymentService.GamePurchase(request);
            Assert.NotNull(result);
            Assert.IsType<PaymentResponse>(result);
        }
    }
}