using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.RoomEvents.Messages;

/// <summary>
/// Модель данных события нового сообщения в комнате
/// </summary>
public class MessageEvent : RoomBaseEvent
{
    /// <summary>
    /// Данные сообщения
    /// </summary>
    public required MessageDto Message { get; init; }
}