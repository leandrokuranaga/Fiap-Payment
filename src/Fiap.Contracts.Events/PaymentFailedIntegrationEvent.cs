namespace Fiap.Contracts.Events;

public record PaymentFailedIntegrationEvent(
        Guid PaymentId,
        int UserId,
        int GameId,
        decimal Price
    );
