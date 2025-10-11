namespace Common.IntegrationEvents.Rooms;

public class RoomDeletedIntegrationEvent
{
    public required Guid Id { get; init; }
}