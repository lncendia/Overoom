using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Rooms.Application.Abstractions.Commands;

namespace Rooms.Infrastructure.Bus.Users;

/// <summary>
/// Обработчик интеграционного события UserInfoChangedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class UserInfoChangedConsumer(ISender mediator) : IConsumer<UserInfoChangedIntegrationEvent>
{
  /// <summary>
  /// Метод обработчик 
  /// </summary>
  /// <param name="context">Контекст сообщения</param>
  public async Task Consume(ConsumeContext<UserInfoChangedIntegrationEvent> context)
  {
    UserInfoChangedIntegrationEvent integrationEvent = context.Message;

    await mediator.Send(new ChangeViewersCommand
    {
      UserId = integrationEvent.Id,
      UserName = integrationEvent.Name,
      PhotoKey = integrationEvent.PhotoKey
    }, context.CancellationToken);
  }
}