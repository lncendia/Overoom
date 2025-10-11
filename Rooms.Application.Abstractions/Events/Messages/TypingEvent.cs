namespace Rooms.Application.Abstractions.Events.Messages;

/// <summary>
/// Модель данных для события набора сообщения пользователем
/// </summary>
public class TypingEvent : RoomBaseEvent
{
    /// <summary>
    /// Идентификатор пользователя, который набирает сообщение
    /// </summary>
    public required Guid Initiator { get; init; }
}