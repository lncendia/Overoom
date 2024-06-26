namespace Films.Application.Abstractions.DTOs.Rooms;

public abstract class RoomDto
{
    public required Guid Id { get; init; }
    public required int ViewersCount { get; init; }
    public required bool IsPrivate { get; init; }
}