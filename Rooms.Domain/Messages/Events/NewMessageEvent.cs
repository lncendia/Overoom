using Common.Domain.Events;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Domain.Messages.Events;

public class NewMessageEvent : DomainEvent, IViewerEvent
{
    public required Room Room { get; init; }
    public required Viewer Viewer { get; init; }
    public required Message Message { get; init; }
}