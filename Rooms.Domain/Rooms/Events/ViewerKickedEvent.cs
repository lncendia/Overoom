using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие исключения зрителя из комнаты
/// </summary>
public class ViewerKickedEvent : DomainEvent, IRoomEvent
{
    /// <summary>
    /// Комната, из которой был исключен зритель
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Исключенный зритель
    /// </summary>
    public required Viewer Target { get; init; }
}