using Microsoft.AspNetCore.SignalR;
using Rooms.Application.Abstractions.RoomEvents;
using Rooms.Application.Abstractions.Services;
using Rooms.Infrastructure.Web.Rooms.Hubs;

namespace Rooms.Infrastructure.Bus.Services;

/// <summary>
/// Отправитель событий комнаты через SignalR хаб
/// </summary>
/// <param name="hubContext">Контекст SignalR хаба для отправки сообщений</param>
public class HubRoomEventSender(IHubContext<RoomHub> hubContext) : IRoomEventSender
{
    /// <inheritdoc/>
    /// <summary>
    /// Отправляет событие комнаты всем клиентам в группе, кроме указанного подключения
    /// </summary>
    public Task SendAsync(RoomBaseEvent @event, Guid roomId, string? excludedConnectionId, CancellationToken cancellationToken = default)
    {
        // Отправка события всем клиентам в группе комнаты, кроме указанного подключения
        return hubContext.Clients
            .GroupExcept(roomId.ToString(), excludedConnectionId ?? string.Empty)
            .SendAsync("Event", @event, cancellationToken);
    }
}