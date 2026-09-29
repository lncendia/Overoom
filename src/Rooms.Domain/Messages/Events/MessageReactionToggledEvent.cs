using Common.Domain.Events;
using Rooms.Domain.Rooms;

namespace Rooms.Domain.Messages.Events;

/// <summary>
/// Событие, представляющее установку или снятие зрителем реакции на сообщение
/// </summary>
public class MessageReactionToggledEvent : DomainEvent
{
  /// <summary>
  /// Комната, в которой находится сообщение
  /// </summary>
  public required Room Room { get; init; }

  /// <summary>
  /// Сообщение, на которое поставлена реакция
  /// </summary>
  public required Message Message { get; init; }

  /// <summary>
  /// Идентификатор зрителя, поставившего или снявшего реакцию
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
