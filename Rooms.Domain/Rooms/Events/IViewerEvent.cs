using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Интерфейс для событий, связанных с действиями зрителя
/// </summary>
public interface IViewerEvent : IRoomEvent
{
    /// <summary>
    /// Зритель, который совершил действие
    /// </summary>
    public Viewer Viewer { get; }
}