using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие отключения зрителя от комнаты
/// </summary>
public class ViewerLeavedEvent : DomainEvent, IViewerEvent
{
    /// <summary>
    /// Комната, которую покинул зритель
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, покинувший комнату
    /// </summary>
    public required Viewer Viewer { get; init; }
}