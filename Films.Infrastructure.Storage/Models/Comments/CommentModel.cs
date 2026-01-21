using Films.Domain.Comments.Snapshots;

namespace Films.Infrastructure.Storage.Models.Comments;

/// <summary>
/// Модель комментария для работы с базой данных.
/// </summary>
public class CommentModel
{
  /// <summary>
  /// Уникальный идентификатор комментария
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Текст комментария (максимальная длина - 1000 символов)
  /// </summary>
  public required string Text { get; set; }

  /// <summary>
  /// Дата и время создания комментария
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// Идентификатор пользователя, оставившего комментарий (может быть null)
  /// </summary>
  public Guid UserId { get; set; }

  /// <summary>
  /// Идентификатор фильма, к которому относится комментарий
  /// </summary>
  public Guid FilmId { get; set; }
  
  /// <summary>
  /// Дата и время изменения модели
  /// </summary>
  public DateTime ModifiedAt { get; set; }

  public CommentSnapshot GetSnapshot() => new()
  {
    Id = Id,
    FilmId = FilmId,
    UserId = UserId,
    Text = Text,
    CreatedAt = CreatedAt
  };

  public void UpdateFromSnapshot(CommentSnapshot snapshot)
  {
    FilmId = snapshot.FilmId;
    UserId = snapshot.UserId;
    Text = snapshot.Text;
    CreatedAt = snapshot.CreatedAt;
  }
}