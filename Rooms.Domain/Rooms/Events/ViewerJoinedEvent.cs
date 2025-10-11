using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие подключения зрителя к комнате
/// </summary>
public class ViewerJoinedEvent : DomainEvent, IViewerEvent
{
    /// <summary>
    /// Комната, к которой подключился зритель
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Подключившийся зритель
    /// </summary>
    public required Viewer Viewer { get; init; }
}