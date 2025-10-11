using Common.Domain.Events;
using Films.Domain.Films;
using Films.Domain.Users;

namespace Films.Domain.Rooms.Events;

/// <summary>
/// Класс, представляющий событие создания новой комнаты с фильмом.
/// </summary>
public class RoomCreatedEvent(Room room, User owner, Film film) : DomainEvent
{
    /// <summary>
    /// Комната.
    /// </summary>
    public Room Room { get; } = room;
    
    /// <summary>
    /// Фильм.
    /// </summary>
    public Film Film { get; } = film;

    /// <summary>
    /// Создатель комнаты.
    /// </summary>
    public User Owner { get; } = owner;
}