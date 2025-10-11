using Rooms.Application.Abstractions.Events.Player;

namespace Rooms.Application.Abstractions.DTOs;

/// <summary>
/// Данные для синхронизации
/// </summary>
public class RoomSyncDto
{
    /// <summary>
    /// Событие паузы/возобновления воспроизведения медиа-контента
    /// </summary>
    public required PauseEvent PauseEvent { get; init; }
    
    /// <summary>
    /// Событие скорости воспроизведения медиа-контента
    /// </summary>
    public required SpeedEvent SpeedEvent { get; init; }
    
    /// <summary>
    /// Событие временной позиции воспроизведения
    /// </summary>
    public required TimeLineEvent TimeLineEvent { get; init; }
    
    /// <summary>
    /// Событие выбора эпизода или серии (опционально)
    /// </summary>
    public EpisodeEvent? EpisodeEvent { get; init; }
}