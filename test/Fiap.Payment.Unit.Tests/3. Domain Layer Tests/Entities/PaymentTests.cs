using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;
using Fiap.Payment.Domain.SeedWork;
using PaymentEntity = Fiap.Payment.Domain.PaymentAggregate.Payment;

namespace Fiap.Payment.Unit.Tests._3._Domain_Layer_Tests.Entities
{
    public class PaymentTests
    {
        [Fact]
        public void Payment_Create_ShouldCreateSuccessfully_WhenValidParameters()
        {
            // Arrange
            var userId = 1;
            var gameId = 6;
            var price = 29.99m;

            // Act
            var payment = PaymentEntity.Create(userId, gameId, price);

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(userId, payment.UserId);
            Assert.Equal(gameId, payment.GameId);
            Assert.NotNull(payment.Price);
            Assert.Equal(price, payment.Price.Value);
            Assert.Equal("BRL", payment.Price.Currency);
            Assert.NotEqual(Guid.Empty, payment.Id);
            Assert.True(payment.CreatedAt <= DateTime.UtcNow);
            Assert.True(payment.UpdatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Payment_Create_ShouldCreateWithUniqueId()
        {
            // Act
            var payment1 = PaymentEntity.Create(1, 6, 29.99m);
            var payment2 = PaymentEntity.Create(2, 7, 39.99m);

            // Assert
            Assert.NotEqual(payment1.Id, payment2.Id);
        }

        [Fact]
        public void Payment_ShouldInheritFromEntity()
        {
            // Arrange
            var payment = new PaymentEntity();

            // Act & Assert
            Assert.IsAssignableFrom<Abp.Domain.Entities.Entity<Guid>>(payment);
        }

        [Fact]
        public void Payment_ShouldImplementAggregateRoot()
        {
            // Arrange
            var payment = new PaymentEntity();

            // Act & Assert
            Assert.IsAssignableFrom<IAggregateRoot>(payment);
        }

        [Fact]
        public void Payment_ShouldCreateSuccessfully_WithParameterlessConstructor()
        {
            // Act
            var payment = new PaymentEntity();

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(0, payment.UserId);
            Assert.Equal(0, payment.GameId);
            Assert.Null(payment.Price);
            Assert.Equal(DateTime.MinValue, payment.CreatedAt);
            Assert.Equal(DateTime.MinValue, payment.UpdatedAt);
        }

        [Fact]
        public void Payment_Create_ShouldCreateMoneyValueObject()
        {
            // Arrange
            var price = 99.99m;

            // Act
            var payment = PaymentEntity.Create(1, 1, price);

            // Assert
            Assert.NotNull(payment.Price);
            Assert.IsType<Money>(payment.Price);
            Assert.Equal(price, payment.Price.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0.01)]
        [InlineData(1.50)]
        [InlineData(999.99)]
        [InlineData(10000)]
        public void Payment_Create_ShouldAcceptValidPrices(decimal price)
        {
            // Act
            var payment = PaymentEntity.Create(1, 1, price);

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(price, payment.Price.Value);
        }
    }
}