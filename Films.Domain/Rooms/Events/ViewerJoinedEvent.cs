using Common.Domain.Events;
using Films.Domain.Users;

namespace Films.Domain.Rooms.Events;

/// <summary>
/// Доменное событие, возникающее когда зритель подключается к комнате
/// Содержит информацию о комнате и данные подключившегося зрителя
/// </summary>
/// <param name="room">Комната, к которой подключился зритель</param>
/// <param name="viewer">Данные зрителя, который подключился к комнате</param>
public class ViewerJoinedEvent(Room room, User viewer) : DomainEvent
{
    /// <summary>
    /// Комната, к которой подключился зритель
    /// </summary>
    public Room Room { get; } = room;
    
    /// <summary>
    /// Данные зрителя, который подключился к комнате
    /// </summary>
    public User Viewer { get; } = viewer;
}