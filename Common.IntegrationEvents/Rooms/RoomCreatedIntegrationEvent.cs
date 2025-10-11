namespace Common.IntegrationEvents.Rooms;

public class RoomCreatedIntegrationEvent
{
    public required Guid Id { get; init; }

    public required Guid FilmId { get; init; }

    public required bool IsSerial { get; init; }

    public required Viewer Owner { get; init; }
}