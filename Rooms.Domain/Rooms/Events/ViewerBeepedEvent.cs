using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие отправки бипа (звукового сигнала) одним зрителем другому
/// </summary>
public class ViewerBeepedEvent : DomainEvent, IRoomEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, отправивший бип
    /// </summary>
    public required Viewer Initiator { get; init; }
    
    /// <summary>
    /// Зритель, получивший бип
    /// </summary>
    public required Viewer Target { get; init; }
}