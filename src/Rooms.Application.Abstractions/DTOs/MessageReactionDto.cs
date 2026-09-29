namespace Rooms.Application.Abstractions.DTOs;

/// <summary>
/// Реакция зрителя на сообщение
/// </summary>
public class MessageReactionDto
{
  /// <summary>
  /// Идентификатор зрителя
  /// </summary>
  public required Guid UserId { get; init; }

  /// <summary>
  /// Код реакции
  /// </summary>
  public required string Reaction { get; init; }
}
