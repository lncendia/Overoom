using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения скорости воспроизведения зрителем
/// </summary>
public class ViewerSpeedChangedEvent : DomainEvent, IViewerEvent, IPlayerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший скорость воспроизведения
    /// </summary>
    public required Viewer Viewer { get; init; }
    
    /// <summary>
    /// Флаг, указывающий является ли событие результатом синхронизации
    /// (а не ручного действия пользователя)
    /// </summary>
    public bool IsSyncEvent { get; init; }
}