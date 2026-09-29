using Common.Domain.Events;

namespace Films.Domain.CommentReactions.Events;

/// <summary>
/// Событие, представляющее снятие пользователем реакции с комментария
/// </summary>
public class CommentReactionRemovedEvent(CommentReaction reaction) : DomainEvent
{
  /// <summary>
  /// Снятая реакция
  /// </summary>
  public CommentReaction Reaction { get; } = reaction;
}
