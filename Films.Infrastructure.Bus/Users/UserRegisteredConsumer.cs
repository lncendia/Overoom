using Common.IntegrationEvents.Users;
using Films.Application.Abstractions.Commands.Profile;
using MassTransit;
using MediatR;

namespace Films.Infrastructure.Bus.Users;

/// <summary>
/// Обработчик интеграционного события UserCreatedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class UserRegisteredConsumer(ISender mediator) : IConsumer<UserRegisteredIntegrationEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        // Получаем данные события
        var integrationEvent = context.Message;

        // Отправляем команду на обработку события
        await mediator.Send(new AddUserCommand
        {
            Id = integrationEvent.Id,
            UserName = integrationEvent.Name,
            PhotoKey = integrationEvent.PhotoKey
        }, context.CancellationToken);
    }
}