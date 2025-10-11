using Common.Domain.Rooms;

namespace Common.IntegrationEvents.Rooms;

public class Viewer
{
    public required Guid Id { get; init; }
    
    public string? PhotoKey { get; init; }
    
    public required string UserName { get; init; }
    
    /// <summary>
    /// Разрешение на совершение звукового сигнала.
    /// </summary>
    public required RoomSettings Settings { get; init; }
}