using Common.Domain.Rooms;
using Films.Domain.Users.ValueObjects;

namespace Films.Domain.Users.Snapshots;

public class UserSnapshot
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public string? PhotoKey { get; init; }
    public required RoomSettings RoomSettings { get; init; }
    public required IReadOnlyCollection<FilmNote> Watchlist { get; init; }
    public required IReadOnlyCollection<FilmNote> History { get; init; }
    public required IReadOnlyCollection<string> Genres { get; init; }
}
