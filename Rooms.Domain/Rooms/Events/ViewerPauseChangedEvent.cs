using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения состояния паузы зрителем
/// </summary>
public class ViewerPauseChangedEvent : DomainEvent, IViewerEvent, IPlayerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший состояние паузы
    /// </summary>
    public required Viewer Viewer { get; init; }
    
    /// <summary>
    /// Флаг, указывающий является ли событие результатом синхронизации
    /// (а не ручного действия пользователя)
    /// </summary>
    public required bool IsSyncEvent { get; init; }
    
    /// <summary>
    /// Флаг, указывающий, что пауза вызвана буферизацией (дозагрузкой контента)
    /// </summary>
    public required bool Buffering { get; init; }
}