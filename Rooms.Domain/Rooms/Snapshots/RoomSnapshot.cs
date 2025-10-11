namespace Rooms.Domain.Rooms.Snapshots;

public record RoomSnapshot
{
    public required Guid Id { get; init; }
    public required Guid FilmId { get; init; }
    public required bool IsSerial { get; init; }
    public required Guid OwnerId { get; init; }
    public required IReadOnlyCollection<ViewerSnapshot> Viewers { get; init; }
}