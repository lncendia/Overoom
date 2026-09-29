using Common.Domain.Aggregates;

using Films.Domain.CommentReactions.Events;
using Films.Domain.CommentReactions.Exceptions;
using Films.Domain.Comments;
using Films.Domain.Users;

namespace Films.Domain.CommentReactions;

/// <summary>
/// Реакция пользователя на комментарий.
/// </summary>
/// <remarks>
/// Пользователь может поставить на комментарий несколько разных реакций, но каждую — только один раз.
/// </remarks>
public partial class CommentReaction : AggregateRoot
{
  #region Поля и свойства

  /// <summary>
  /// Идентификатор комментария, к которому относится реакция.
  /// </summary>
  public Guid CommentId { get; }

  /// <summary>
  /// Идентификатор пользователя, поставившего реакцию.
  /// </summary>
  public Guid UserId { get; }

  /// <summary>
  /// Код реакции (см. <see cref="ReactionCodes"/>).
  /// </summary>
  public string Reaction { get; }

  /// <summary>
  /// Время установки реакции.
  /// </summary>
  public DateTime CreatedAt { get; } = DateTime.UtcNow;

  #endregion

  #region Методы

  /// <summary>
  /// Снимает реакцию. После вызова агрегат должен быть удалён из репозитория.
  /// </summary>
  public void Remove()
  {
    AddDomainEvent(new CommentReactionRemovedEvent(this));
  }

  #endregion

  /// <summary>
  /// Инициализирует новый экземпляр класса <see cref="CommentReaction"/>.
  /// </summary>
  /// <param name="id">Идентификатор реакции.</param>
  /// <param name="comment">Комментарий, на который ставится реакция.</param>
  /// <param name="user">Пользователь, который ставит реакцию.</param>
  /// <param name="reaction">Код реакции.</param>
  /// <exception cref="UnknownReactionException">Если реакция не входит в число допустимых.</exception>
  public CommentReaction(Guid id, Comment comment, User user, string reaction) : base(id)
  {
    if (!ReactionCodes.All.Contains(reaction)) throw new UnknownReactionException(reaction);

    CommentId = comment.Id;
    UserId = user.Id;
    Reaction = reaction;
  }
}
