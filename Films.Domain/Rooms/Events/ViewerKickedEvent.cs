using Common.Domain.Events;

namespace Films.Domain.Rooms.Events;

/// <summary>
/// Доменное событие, возникающее когда зритель принудительно исключен из комнаты
/// Содержит информацию о комнате и идентификаторе исключенного зрителя
/// </summary>
/// <param name="room">Комната, из которой был исключен зритель</param>
/// <param name="viewerId">Идентификатор зрителя, который был исключен</param>
public class ViewerKickedEvent(Room room, Guid viewerId) : DomainEvent
{
    /// <summary>
    /// Комната, из которой был исключен зритель
    /// </summary>
    public Room Room { get; } = room;
    
    /// <summary>
    /// Идентификатор зрителя, который был исключен из комнаты
    /// </summary>
    public Guid ViewerId { get; } = viewerId;
}