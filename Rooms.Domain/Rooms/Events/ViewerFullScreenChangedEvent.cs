using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения полноэкранного режима зрителем
/// </summary>
public class ViewerFullScreenChangedEvent : DomainEvent, IRoomEvent, IPlayerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший полноэкранный режим
    /// </summary>
    public required Viewer Viewer { get; init; }
    
    /// <summary>
    /// Флаг, указывающий является ли событие результатом синхронизации
    /// (а не ручного действия пользователя)
    /// </summary>
    public required bool IsSyncEvent { get; init; }
}