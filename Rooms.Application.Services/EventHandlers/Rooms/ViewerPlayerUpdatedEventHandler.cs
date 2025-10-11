using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Common.Domain.Events;
using MassTransit;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Player;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик событий обновления данных воспроизведения зрителя
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class ViewerPlayerUpdatedEventHandler(IPublishEndpoint publish, IScopedContext context)
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

        var connectionId = context.Current.Get<string>(Constants.CurrentConnectionIdKey);

        using var _ = context.CreateScope();

        context.Current.SetRoomHeaders(@event.Aggregate.Id, connectionId);

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