namespace Common.IntegrationEvents.Rooms;

public class RoomViewerJoinedIntegrationEvent
{
    public required Guid RoomId { get; init; }
    public required Viewer Viewer { get; init; }
}