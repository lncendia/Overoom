namespace Films.Application.Abstractions.DTOs.Comments;

/// <summary>
/// Сводка по одной реакции на комментарий
/// </summary>
public class CommentReactionDto
{
  /// <summary>
  /// Код реакции
  /// </summary>
  public required string Reaction { get; init; }

  /// <summary>
  /// Количество пользователей, поставивших реакцию
  /// </summary>
  public required int Count { get; init; }

  /// <summary>
  /// Поставил ли реакцию текущий пользователь
  /// </summary>
  public required bool Reacted { get; init; }
}
