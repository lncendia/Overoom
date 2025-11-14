using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Common.Domain.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.RoomEvents.Player;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик обновления данных воспроизведения зрителя
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
/// <param name="context">Контекст выполняемой области</param>
public class ViewerPlayerUpdatedEventHandler(IRoomEventSender eventSender, IScopedContext context)
    : AfterSaveNotificationHandler<SaveEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие сохранения комнаты и публикует события обновления для всех зрителей
    /// </summary>
    /// <param name="event">Событие сохранения комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(SaveEvent<Room> @event, CancellationToken cancellationToken)
    {
        // Если мы не в контексте (событие пришло не через хаб) - не продолжаем
        if (!context.InScope) return;

        var excludedConnectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);

        foreach (var viewer in @event.Aggregate.Viewers.Values)
        {
            await Execute(@event.Aggregate.Id, excludedConnectionId, viewer, cancellationToken);
        }
    }

    /// <summary>
    /// Создает и публикует событие обновления для конкретного зрителя
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты</param>
    /// <param name="excludedConnectionId">Идентификатор подключения, которое следует исключить из получателей</param>
    /// <param name="viewer">Данные зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    private async Task Execute(Guid roomId, string excludedConnectionId, Viewer viewer, CancellationToken cancellationToken)
    {
        var updatedFields = new List<string>();

        var publishEvent = new UpdateViewerPlayerEvent
        {
            Id = viewer.Id,
            UpdatedFields = updatedFields
        };

        // Обработка измененных свойств зрителя
        foreach (var property in viewer.ChangedProperties)
        {
            var propertyToLower = LowercaseFirstLetter(property);
            switch (property)
            {
                case nameof(viewer.OnPause):
                    publishEvent.OnPause = viewer.OnPause;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.FullScreen):
                    publishEvent.FullScreen = viewer.FullScreen;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.TimeLine):
                    publishEvent.TimeLine = viewer.TimeLine.Ticks;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.Speed):
                    publishEvent.Speed = viewer.Speed;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.Season):
                    publishEvent.Season = viewer.Season;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.Episode):
                    publishEvent.Episode = viewer.Episode;
                    updatedFields.Add(propertyToLower);
                    break;
            }
        }

        // Публикация события только если были изменения
        if (updatedFields.Count == 0) return;
        await eventSender.SendAsync(publishEvent, roomId, excludedConnectionId, cancellationToken);
    }

    /// <summary>
    /// Преобразует первую букву строки в нижний регистр
    /// </summary>
    /// <param name="s">Входная строка</param>
    /// <returns>Строка с первой буквой в нижнем регистре</returns>
    private static string LowercaseFirstLetter(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        return char.ToLower(s[0]) + s[1..];
    }
}