namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Базовый интерфейс для всех событий, связанных с комнатой
/// </summary>
public interface IRoomEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public Room Room { get; }
}