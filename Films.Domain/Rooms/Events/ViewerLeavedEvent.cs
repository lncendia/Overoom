using Common.Domain.Events;

namespace Films.Domain.Rooms.Events;

/// <summary>
/// Доменное событие, возникающее когда зритель добровольно покидает комнату
/// Содержит информацию о комнате и идентификаторе покинувшего зрителя
/// </summary>
/// <param name="room">Комната, которую покинул зритель</param>
/// <param name="viewerId">Идентификатор зрителя, который покинул комнату</param>
public class ViewerLeavedEvent(Room room, Guid viewerId) : DomainEvent
{
    /// <summary>
    /// Комната, которую покинул зритель
    /// </summary>
    public Room Room { get; } = room;
    
    /// <summary>
    /// Идентификатор зрителя, который покинул комнату
    /// </summary>
    public Guid ViewerId { get; } = viewerId;
}