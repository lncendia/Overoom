using MediatR;

namespace Films.Application.Abstractions.Commands.Comments;

/// <summary>
/// Команда на установку или снятие реакции на комментарий
/// </summary>
public class SetCommentReactionCommand : IRequest
{
  /// <summary>
  /// Идентификатор пользователя
  /// </summary>
  public required Guid UserId { get; init; }

  /// <summary>
  /// Идентификатор фильма, к которому относится комментарий
  /// </summary>
  public required Guid FilmId { get; init; }

  /// <summary>
  /// Идентификатор комментария
  /// </summary>
  public required Guid CommentId { get; init; }

  /// <summary>
  /// Код реакции
  /// </summary>
  public required string Reaction { get; init; }

  /// <summary>
  /// true — поставить реакцию, false — снять
  /// </summary>
  public required bool IsSet { get; init; }
}
