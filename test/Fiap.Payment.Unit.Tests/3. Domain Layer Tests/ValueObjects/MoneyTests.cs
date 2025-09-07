using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;
using Fiap.Payment.Domain.SeedWork.Exceptions;

namespace Fiap.Payment.Unit.Tests._3._Domain_Layer_Tests.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void Money_ShouldCreateSuccessfully_WhenValidValues()
        {
            // Arrange
            decimal value = 100.50m;
            string currency = "USD";

            // Act
            var money = new Money(value, currency);

            // Assert
            Assert.NotNull(money);
            Assert.Equal(value, money.Value);
            Assert.Equal(currency, money.Currency);
        }

        [Fact]
        public void Money_ShouldCreateWithDefaultCurrency_WhenOnlyValueProvided()
        {
            // Arrange
            decimal value = 50.00m;

            // Act
            var money = new Money(value);

            // Assert
            Assert.NotNull(money);
            Assert.Equal(value, money.Value);
            Assert.Equal("BRL", money.Currency);
        }

        [Fact]
        public void Money_ShouldThrowException_WhenNegativeValue()
        {
            // Arrange
            decimal negativeValue = -10.00m;

            // Act & Assert
            var exception = Assert.Throws<BusinessRulesException>(() => new Money(negativeValue));
            Assert.Contains("must be greater than or equal to 0", exception.Message);
        }

        [Fact]
        public void Money_ShouldThrowException_WhenInvalidCurrency()
        {
            // Arrange
            decimal value = 100.00m;
            string invalidCurrency = "XXX";

            // Act & Assert
            var exception = Assert.Throws<BusinessRulesException>(() => new Money(value, invalidCurrency));
            Assert.Contains("Invalid currency", exception.Message);
        }

        [Fact]
        public void IsValidCurrency_ShouldReturnTrue_WhenValidCurrency()
        {
            // Act & Assert
            Assert.True(Money.IsValidCurrency("USD"));
            Assert.True(Money.IsValidCurrency("EUR"));
            Assert.True(Money.IsValidCurrency("BRL"));
            Assert.True(Money.IsValidCurrency("usd")); // Should handle case insensitive
        }

        [Fact]
        public void IsValidCurrency_ShouldReturnFalse_WhenInvalidCurrency()
        {
            // Act & Assert
            Assert.False(Money.IsValidCurrency("XXX"));
            Assert.False(Money.IsValidCurrency(""));
        }
    }
}