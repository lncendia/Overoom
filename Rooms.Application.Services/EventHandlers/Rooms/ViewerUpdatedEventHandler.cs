using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Common.Domain.Events;
using MassTransit;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Room;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик событий обновления данных зрителя
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class ViewerUpdatedEventHandler(IPublishEndpoint publish, IScopedContext context)
    : AfterSaveNotificationHandler<SaveEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие сохранения комнаты и публикует события обновления для всех зрителей
    /// </summary>
    /// <param name="event">Событие сохранения комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(SaveEvent<Room> @event, CancellationToken cancellationToken)
    {
        using var _ = context.CreateScope();
        
        context.Current.SetRoomHeaders(@event.Aggregate.Id);

        foreach (var viewer in @event.Aggregate.Viewers.Values)
        {
            await Execute(viewer, cancellationToken);
        }
    }

    /// <summary>
    /// Создает и публикует событие обновления для конкретного зрителя
    /// </summary>
    /// <param name="viewer">Данные зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    private async Task Execute(Viewer viewer, CancellationToken cancellationToken)
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
        await publish.Publish<RoomBaseEvent>(publishEvent, cancellationToken);
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