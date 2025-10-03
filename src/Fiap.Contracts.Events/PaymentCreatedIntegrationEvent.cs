namespace Fiap.Contracts.Events;

public record PaymentCreatedIntegrationEvent(
        Guid PaymentId,
        int UserId,
        int GameId,
        decimal Price
    );
