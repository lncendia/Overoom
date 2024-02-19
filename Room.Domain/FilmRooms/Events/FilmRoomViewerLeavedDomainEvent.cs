using Room.Domain.Abstractions;

namespace Room.Domain.FilmRooms.Events;

public class FilmRoomViewerLeavedDomainEvent(FilmRoom room, Guid viewerId) : IDomainEvent
{
    public FilmRoom Room { get; } = room;
    public Guid ViewerId { get; } = viewerId;
}