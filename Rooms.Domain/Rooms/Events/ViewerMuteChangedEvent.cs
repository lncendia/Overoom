using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения состояния звука (mute) зрителем
/// </summary>
public class ViewerMuteChangedEvent : DomainEvent, IViewerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший состояние звука
    /// </summary>
    public required Viewer Viewer { get; init; }
}