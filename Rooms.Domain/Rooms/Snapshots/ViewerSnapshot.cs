using Common.Domain.Rooms;

namespace Rooms.Domain.Rooms.Snapshots;

public record ViewerSnapshot
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string? PhotoKey { get; init; }
    public required bool Online { get; init; }
    public required bool FullScreen { get; init; }
    public required bool OnPause { get; init; }
    public required TimeSpan TimeLine { get; init; }
    public required bool Muted { get; init; }
    public required double Speed { get; init; }
    public required int? Season { get; init; }
    public required int? Episode { get; init; }
    public required IReadOnlySet<string> Tags { get; init; }
    public required IReadOnlyDictionary<string, int> Statistic { get; init; }
    public required RoomSettings Settings { get; init; }
}