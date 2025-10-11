using Common.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Abstractions.DTOs;

/// <summary>
/// DTO объект зрителя для API
/// </summary>
/// <remarks>
/// Содержит всю необходимую информацию о зрителе для отображения в интерфейсе
/// </remarks>
public class ViewerDto
{
    /// <summary>
    /// Уникальный ID зрителя
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Никнейм пользователя
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Ключ аватара в хранилище (может быть null)
    /// </summary>
    public string? PhotoKey { get; init; }

    /// <summary>
    /// Флаг паузы (true - стоит на паузе)
    /// </summary>
    public required bool OnPause { get; init; }

    /// <summary>
    /// Онлайн статус
    /// </summary>
    public required bool Online { get; init; }

    /// <summary>
    /// Полноэкранный режим
    /// </summary>
    public required bool FullScreen { get; init; }

    /// <summary>
    /// Текущая позиция просмотра в тиках (1 тик = 100 наносекунд)
    /// </summary>
    public required long TimeLine { get; init; }
    
    /// <summary>
    /// Текущая скорость воспроизведения
    /// </summary>
    public required double Speed { get; init; }

    /// <summary>
    /// Права пользователя на действия в комнате
    /// </summary>
    public required RoomSettings Settings { get; init; }

    /// <summary>
    /// Текущий сезон (для сериалов)
    /// </summary>
    public int? Season { get; init; }

    /// <summary>
    /// Текущая серия (для сериалов)
    /// </summary>
    public int? Episode { get; init; }

    /// <summary>
    /// Список тегов зрителя
    /// </summary>
    /// <remarks>
    /// Теги показывают особые статусы (например, "Ведущий", "Оффлайн" и т.д.)
    /// </remarks>
    public required IReadOnlyList<ViewerTagDto> Tags { get; init; }

    /// <summary>
    /// Создает DTO из доменной модели зрителя
    /// </summary>
    /// <param name="viewer">Доменная модель зрителя</param>
    /// <returns>Новый экземпляр DTO</returns>
    public static ViewerDto Create(Viewer viewer) => new()
    {
        Id = viewer.Id,
        UserName = viewer.UserName,
        PhotoKey = viewer.PhotoKey,
        OnPause = viewer.OnPause,
        FullScreen = viewer.FullScreen,
        Online = viewer.Online,
        TimeLine = viewer.TimeLine.Ticks,
        Speed = viewer.Speed,
        Season = viewer.Season,
        Episode = viewer.Episode,
        Settings = viewer.Settings,
        Tags = viewer.Tags.Select(t =>
        {
            Constants.ViewerTags.All.TryGetValue(t, out var description);
            return new ViewerTagDto
            {
                Name = t,
                Description = description,
            };
        }).ToArray()
    };
}