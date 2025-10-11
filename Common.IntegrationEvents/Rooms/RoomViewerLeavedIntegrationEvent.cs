namespace Common.IntegrationEvents.Rooms;

public class RoomViewerLeavedIntegrationEvent
{
    public required Guid RoomId { get; init; }
    public required Guid ViewerId { get; init; }
}