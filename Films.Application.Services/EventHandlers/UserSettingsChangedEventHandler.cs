using Common.Application.Events;
using Common.IntegrationEvents.Users;
using Films.Domain.Users.Events;
using MassTransit;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события изменения настроек пользователя
/// </summary>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class UserSettingsChangedEventHandler(IPublishEndpoint publishEndpoint)
    : BeforeSaveNotificationHandler<UserSettingsChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения настроек пользователя и публикует интеграционное событие
    /// </summary>
    /// <param name="notification">Доменное событие изменения настроек</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(UserSettingsChangedEvent notification, CancellationToken cancellationToken)
    {
        // Создаем событие интеграции для синхронизации настроек пользователя между сервисами
        var integrationEvent = new UserSettingsChangedIntegrationEvent
        {
            Id = notification.User.Id,
            Settings = notification.User.RoomSettings
        };

        // Публикуем событие интеграции через MassTransit
        await publishEndpoint.Publish(integrationEvent, cancellationToken: cancellationToken);
    }
}