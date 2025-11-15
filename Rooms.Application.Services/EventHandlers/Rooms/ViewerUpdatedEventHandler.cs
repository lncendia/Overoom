using Common.Application.Events;
using Common.Domain.Events;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.RoomEvents.Room;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик событий обновления данных зрителя
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class ViewerUpdatedEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<SaveEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие сохранения комнаты и публикует события обновления для всех зрителей
    /// </summary>
    /// <param name="event">Событие сохранения комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(SaveEvent<Room> @event, CancellationToken cancellationToken)
    {
        foreach (var viewer in @event.Aggregate.Viewers.Values)
        {
            await Execute(@event.Aggregate.Id, viewer, cancellationToken);
        }
    }

    /// <summary>
    /// Создает и публикует событие обновления для конкретного зрителя
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты</param>
    /// <param name="viewer">Данные зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    private async Task Execute(Guid roomId, Viewer viewer, CancellationToken cancellationToken)
    {
        var updatedFields = new List<string>();

        var publishEvent = new UpdateViewerEvent
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
                case nameof(viewer.Online):
                    publishEvent.Online = viewer.Online;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.Settings):
                    publishEvent.Settings = viewer.Settings;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.UserName):
                    publishEvent.UserName = viewer.UserName;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.PhotoKey):
                    publishEvent.PhotoKey = viewer.PhotoKey;
                    updatedFields.Add(propertyToLower);
                    break;
                case nameof(viewer.Tags):
                    publishEvent.Tags = viewer.Tags.Select(t =>
                    {
                        Constants.ViewerTags.All.TryGetValue(t, out var description);
                        return new ViewerTagDto
                        {
                            Name = t,
                            Description = description
                        };
                    }).ToArray();
                    updatedFields.Add(propertyToLower);
                    break;
            }
        }

        // Публикация события только если были изменения
        if (updatedFields.Count == 0) return;
        await eventSender.SendAsync(publishEvent, roomId, null, cancellationToken);
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