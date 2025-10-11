namespace Rooms.Domain.Messages.Snapshots;

/// <summary>
/// Снимок состояния агрегата Message для хранения или гидратации.
/// </summary>
public record MessageSnapshot
{
    public required Guid Id { get; init; }
    public required Guid RoomId { get; init; }
    public required Guid UserId { get; init; }
    public required string Text { get; init; }
    public required DateTime SentAt { get; init; }
}
