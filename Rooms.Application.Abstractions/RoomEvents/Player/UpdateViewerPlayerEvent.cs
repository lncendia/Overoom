namespace Rooms.Application.Abstractions.RoomEvents.Player;

/// <summary>
/// Модель данных для события обновления параметров плеера зрителя
/// </summary>
public class UpdateViewerPlayerEvent : RoomBaseEvent
{
    /// <summary>Идентификатор зрителя</summary>
    public required Guid Id { get; init; }

    /// <summary>Флаг паузы</summary>
    public bool? OnPause { get; set; }

    /// <summary>Флаг полноэкранного режима</summary>
    public bool? FullScreen { get; set; }

    /// <summary>Позиция на временной шкале</summary>
    public long? TimeLine { get; set; }
    
    /// <summary>Скорость воспроизведения</summary>
    public double? Speed { get; set; }

    /// <summary>Номер сезона</summary>
    public int? Season { get; set; }

    /// <summary>Номер эпизода</summary>
    public int? Episode { get; set; }
    
    /// <summary>Список обновленных полей</summary>
    public required IReadOnlyList<string> UpdatedFields { get; init; }
}