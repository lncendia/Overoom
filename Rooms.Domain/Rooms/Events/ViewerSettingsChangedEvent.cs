using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения настроек зрителя
/// </summary>
public class ViewerSettingsChangedEvent : DomainEvent, IViewerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший настройки
    /// </summary>
    public required Viewer Viewer { get; init; }
}