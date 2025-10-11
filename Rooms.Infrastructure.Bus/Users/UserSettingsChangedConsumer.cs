using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Rooms.Application.Abstractions.Commands;

namespace Rooms.Infrastructure.Bus.Users;

/// <summary>
/// Обработчик интеграционного события изменения настроек пользователя.
/// Синхронизирует настройки пользователя во всех комнатах при получении события из внешней системы
/// </summary>
/// <param name="mediator">Медиатор для отправки внутренних команд приложения</param>
public class UserSettingsChangedConsumer(ISender mediator) : IConsumer<UserSettingsChangedIntegrationEvent>
{
    /// <summary>
    /// Обрабатывает интеграционное событие изменения настроек пользователя
    /// </summary>
    /// <param name="context">Контекст сообщения, содержащий данные события и метаинформацию</param>
    /// <returns>Задача, представляющая асинхронную обработку события</returns>
    public async Task Consume(ConsumeContext<UserSettingsChangedIntegrationEvent> context)
    {
        // Извлекаем данные события из контекста сообщения
        var integrationEvent = context.Message;

        // Преобразуем интеграционное событие во внутреннюю команду приложения
        // и отправляем ее через медиатор для дальнейшей обработки
        await mediator.Send(new ChangeViewersSettingsCommand
        {
            UserId = integrationEvent.Id,
            Settings = integrationEvent.Settings
        }, context.CancellationToken);
    }
}