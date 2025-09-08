using Fiap.Payment.Domain.OutboxAggregate;

namespace Fiap.Payment.Unit.Tests._3._Domain_Layer_Tests.Entities
{
    public class OutboxMessageTests
    {
        [Fact]
        public void OutboxMessage_ShouldCreateSuccessfully_WhenValidParameters()
        {
            // Arrange
            var type = "TestEvent";
            var content = "{\"data\": \"test\"}";
            var occuredOn = DateTime.UtcNow;

            // Act
            var outboxMessage = new OutboxMessage(type, content, occuredOn);

            // Assert
            Assert.NotNull(outboxMessage);
            Assert.Equal(type, outboxMessage.Type);
            Assert.Equal(content, outboxMessage.Content);
            Assert.Equal(occuredOn, outboxMessage.OccuredOn);
            Assert.Null(outboxMessage.ProcessedOn);
        }

        [Fact]
        public void OutboxMessage_ShouldCreateSuccessfully_WithProcessedOn()
        {
            // Arrange
            var type = "TestEvent";
            var content = "{\"data\": \"test\"}";
            var occuredOn = DateTime.UtcNow;
            var processedOn = DateTime.UtcNow.AddMinutes(5);

            // Act
            var outboxMessage = new OutboxMessage(type, content, occuredOn, processedOn);

            // Assert
            Assert.NotNull(outboxMessage);
            Assert.Equal(type, outboxMessage.Type);
            Assert.Equal(content, outboxMessage.Content);
            Assert.Equal(occuredOn, outboxMessage.OccuredOn);
            Assert.Equal(processedOn, outboxMessage.ProcessedOn);
        }

        [Fact]
        public void OutboxMessage_ShouldCreateSuccessfully_WithParameterlessConstructor()
        {
            // Act
            var outboxMessage = new OutboxMessage();

            // Assert
            Assert.NotNull(outboxMessage);
            Assert.Null(outboxMessage.Type);
            Assert.Null(outboxMessage.Content);
            Assert.Equal(DateTime.MinValue, outboxMessage.OccuredOn);
            Assert.Null(outboxMessage.ProcessedOn);
        }

        [Fact]
        public void OutboxMessage_ShouldInheritFromEntity()
        {
            // Arrange
            var outboxMessage = new OutboxMessage();

            // Act & Assert
            Assert.IsAssignableFrom<Abp.Domain.Entities.Entity<Guid>>(outboxMessage);
        }

        [Fact]
        public void OutboxMessage_ShouldHaveDefaultId_WhenCreated()
        {
            // Act
            var message1 = new OutboxMessage("Event1", "Content1", DateTime.UtcNow);
            var message2 = new OutboxMessage("Event2", "Content2", DateTime.UtcNow);

            // Assert - Since ABP doesn't auto-generate IDs by default, they should be Guid.Empty
            Assert.Equal(Guid.Empty, message1.Id);
            Assert.Equal(Guid.Empty, message2.Id);
        }
    }
}