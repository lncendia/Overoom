using Common.Domain.Aggregates;
using Common.Domain.Extensions;

using Films.Domain.Comments.Snapshots;
using Films.Domain.Films;
using Films.Domain.Users;

namespace Films.Domain.Comments;

/// <summary>
/// Класс, представляющий комментарий к фильму.
/// </summary>
public partial class Comment : AggregateRoot
{
  #region Константы

  private const int MaxTextLength = 100;

  #endregion

  /// <summary>
  /// Идентификатор фильма, к которому относится комментарий.
  /// </summary>
  public Guid FilmId { get; }

  /// <summary>
  /// Идентификатор пользователя, создавшего комментарий.
  /// </summary>
  public Guid UserId { get; }

  /// <summary>
  /// Текст комментария.
  /// </summary>
  public string Text { get; }

  /// <summary>
  /// Время создания комментария.
  /// </summary>
  public DateTime CreatedAt { get; } = DateTime.UtcNow;

  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="id">Идентификатор фильма.</param>
  /// <param name="film">Экземпляр фильма, к которому относится комментарий.</param>
  /// <param name="user">Идентификатор пользователя, создавшего комментарий.</param>
  /// <param name="text">Текст комментария.</param>
  public Comment(Guid id, Film film, User user, string text) : base(id)
  {
    FilmId = film.Id;
    UserId = user.Id;
    Text = text.ValidateLength(nameof(Text), MaxTextLength);
  }

  #endregion
}
