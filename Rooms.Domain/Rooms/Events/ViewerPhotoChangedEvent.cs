using Common.Domain.Events;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Событие изменения фотографии профиля зрителя
/// </summary>
public class ViewerPhotoChangedEvent : DomainEvent, IViewerEvent
{
    /// <summary>
    /// Комната, в которой произошло событие
    /// </summary>
    public required Room Room { get; init; }
    
    /// <summary>
    /// Зритель, изменивший фотографию профиля
    /// </summary>
    public required Viewer Viewer { get; init; }
}