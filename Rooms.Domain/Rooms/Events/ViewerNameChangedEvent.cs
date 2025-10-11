using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения имени зрителя
/// </summary>
public class ViewerNameChangedEvent : DomainEvent, IViewerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший имя
    /// </summary>
    public required Viewer Viewer { get; init; }
}