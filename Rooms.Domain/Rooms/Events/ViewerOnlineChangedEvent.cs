using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения онлайн-статуса зрителя
/// </summary>
public class ViewerOnlineChangedEvent : DomainEvent, IViewerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший онлайн-статус
    /// </summary>
    public required Viewer Viewer { get; init; }
}