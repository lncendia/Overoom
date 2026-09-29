namespace Rooms.Application.Abstractions.RoomEvents.Messages;

/// <summary>
/// Событие установки или снятия зрителем реакции на сообщение
/// </summary>
public class MessageReactionEvent : RoomBaseEvent
{
  /// <summary>
  /// Идентификатор сообщения
  /// </summary>
  public required Guid MessageId { get; init; }

  /// <summary>
  /// Идентификатор зрителя
  /// </summary>
  public required Guid ViewerId { get; init; }

  /// <summary>
  /// Код реакции
  /// </summary>
  public required string Reaction { get; init; }

  /// <summary>
  /// true, если реакция поставлена, false, если снята
  /// </summary>
  public required bool Added { get; init; }
}
