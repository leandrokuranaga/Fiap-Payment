namespace Fiap.Contracts.Events;

public record PaymentSuccessfulIntegrationEvent(
        Guid PaymentId,
        int UserId,
        int GameId,
        decimal Price
    );
