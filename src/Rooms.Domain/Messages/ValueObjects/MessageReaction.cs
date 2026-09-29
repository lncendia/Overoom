namespace Rooms.Domain.Messages.ValueObjects;

/// <summary>
/// Реакция зрителя на сообщение.
/// </summary>
public record MessageReaction
{
  /// <summary>
  /// Идентификатор зрителя, поставившего реакцию.
  /// </summary>
  public required Guid UserId { get; init; }

  /// <summary>
  /// Код реакции (см. <see cref="MessageReactions"/>).
  /// </summary>
  public required string Reaction { get; init; }
}
