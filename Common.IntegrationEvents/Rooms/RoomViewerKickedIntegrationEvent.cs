namespace Common.IntegrationEvents.Rooms;

public class RoomViewerKickedIntegrationEvent
{
    public required Guid RoomId { get; init; }
    public required Guid ViewerId { get; init; }
}