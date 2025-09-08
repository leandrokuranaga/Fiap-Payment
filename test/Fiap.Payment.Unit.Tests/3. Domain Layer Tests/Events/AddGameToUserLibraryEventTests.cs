using Fiap.Payment.Domain.PaymentAggregate.Events;
using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;

namespace Fiap.Payment.Unit.Tests._3._Domain_Layer_Tests.Events
{
    public class AddGameToUserLibraryEventTests
    {
        [Fact]
        public void AddGameToUserLibraryEvent_ShouldCreateSuccessfully()
        {
            // Arrange
            var gameId = 6;
            var userId = 1;
            var purchaseDate = DateTime.UtcNow;
            var pricePaid = new Money(29.99m, "USD");

            // Act
            var eventObj = new AddGameToUserLibraryEvent
            {
                GameId = gameId,
                UserId = userId,
                PurchaseDate = purchaseDate,
                PricePaid = pricePaid
            };

            // Assert
            Assert.NotNull(eventObj);
            Assert.Equal(gameId, eventObj.GameId);
            Assert.Equal(userId, eventObj.UserId);
            Assert.Equal(purchaseDate, eventObj.PurchaseDate);
            Assert.Equal(pricePaid, eventObj.PricePaid);
        }

        [Fact]
        public void AddGameToUserLibraryEvent_ShouldBeRecord()
        {
            // Arrange & Act
            var event1 = new AddGameToUserLibraryEvent 
            { 
                GameId = 1, 
                UserId = 1, 
                PurchaseDate = DateTime.Today, 
                PricePaid = new Money(10m) 
            };
            
            var event2 = new AddGameToUserLibraryEvent 
            { 
                GameId = 1, 
                UserId = 1, 
                PurchaseDate = DateTime.Today, 
                PricePaid = new Money(10m) 
            };

            // Assert
            Assert.Equal(event1.GameId, event2.GameId);
            Assert.Equal(event1.UserId, event2.UserId);
            Assert.Equal(event1.PurchaseDate, event2.PurchaseDate);
        }
    }
}