using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие отправки крика одним зрителем другому
/// </summary>
public class ViewerScreamedEvent : DomainEvent, IRoomEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, отправивший крик
    /// </summary>
    public required Viewer Initiator { get; init; }
    
    /// <summary>
    /// Зритель, получивший крик
    /// </summary>
    public required Viewer Target { get; init; }
}