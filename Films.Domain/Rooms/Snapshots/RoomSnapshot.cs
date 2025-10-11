namespace Films.Domain.Rooms.Snapshots;

public class RoomSnapshot
{
    public required Guid Id { get; init; }
    public required Guid FilmId { get; init; }
    public required string? Code { get; init; }
    public required Guid OwnerId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyCollection<Guid> Viewers { get; init; }
    public required IReadOnlyCollection<Guid> BannedUsers { get; init; }
}
