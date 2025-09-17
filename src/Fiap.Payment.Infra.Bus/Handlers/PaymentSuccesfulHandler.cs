using Fiap.Contracts.Events;
using Fiap.Payment.Domain.PaymentAggregate.Events;
using Fiap.Payment.Domain.PaymentAggregate.ValueObjects;
using Fiap.Payment.Domain.SeedWork;
using Rebus.Handlers;
using System.Text.Json;

namespace Fiap.Payment.Infra.Bus.Handlers;
public class PaymentSuccesfulHandler(IEventStoreRepository eventStoreRepository) : IHandleMessages<PaymentSuccessfulIntegrationEvent>
{
    public async Task Handle(PaymentSuccessfulIntegrationEvent message)
    {
        var paymentSuccessful = new PaymentSuccessfulEvent
        {
            Id = Guid.NewGuid(),
            PaymentId = message.PaymentId,
            UserId = message.UserId,
            GameId = message.GameId,
            Price = new Money(message.Price),
            OccurredOn = DateTime.UtcNow,
            StreamName = $"Payment-{message.PaymentId}",
            Type = nameof(Domain.PaymentAggregate.Payment),
            Data = JsonSerializer.Serialize(new
            {
                message.PaymentId,
                message.UserId,
                message.GameId,
                message.Price
            })
        };

        await eventStoreRepository.SaveAsync(paymentSuccessful);
    }
}