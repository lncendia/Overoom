using Common.Application.DTOs;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.RoomEvents.Messages;

/// <summary>
/// Модель данных события получения истории сообщений
/// </summary>
public class MessagesEvent : RoomBaseEvent
{
    /// <summary>
    /// Сообщения
    /// </summary>
    public required CountResult<MessageDto> Messages { get; init; }
}