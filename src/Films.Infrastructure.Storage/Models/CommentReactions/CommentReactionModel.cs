using Common.Infrastructure.Repositories.Models;

using Films.Domain.CommentReactions.Snapshots;

namespace Films.Infrastructure.Storage.Models.CommentReactions;

/// <summary>
/// Модель реакции на комментарий для работы с базой данных.
/// </summary>
public class CommentReactionModel : IModel<CommentReactionSnapshot>
{
  #region Поля и свойства

  /// <summary>
  /// Идентификатор комментария, к которому относится реакция
  /// </summary>
  public Guid CommentId { get; set; }

  /// <summary>
  /// Идентификатор пользователя, поставившего реакцию
  /// </summary>
  public Guid UserId { get; set; }

  /// <summary>
  /// Код реакции
  /// </summary>
  public string Reaction { get; set; } = null!;

  /// <summary>
  /// Дата и время установки реакции
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// Дата и время изменения модели
  /// </summary>
  public DateTime ModifiedAt { get; set; }

  #endregion

  #region IModel

  /// <summary>
  /// Уникальный идентификатор реакции
  /// </summary>
  public required Guid Id { get; init; }

  public void UpdateFromSnapshot(CommentReactionSnapshot snapshot)
  {
    CommentId = snapshot.CommentId;
    UserId = snapshot.UserId;
    Reaction = snapshot.Reaction;
    CreatedAt = snapshot.CreatedAt;
  }

  public CommentReactionSnapshot GetSnapshot()
  {
    return new CommentReactionSnapshot
    {
      Id = Id,
      CommentId = CommentId,
      UserId = UserId,
      Reaction = Reaction,
      CreatedAt = CreatedAt
    };
  }

  #endregion
}
