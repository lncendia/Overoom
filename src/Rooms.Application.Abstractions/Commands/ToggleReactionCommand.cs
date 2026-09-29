namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на установку или снятие реакции на сообщение
/// </summary>
public class ToggleReactionCommand : RoomBaseCommand
{
  /// <summary>
  /// Идентификатор сообщения
  /// </summary>
  public required Guid MessageId { get; init; }

  /// <summary>
  /// Код реакции
  /// </summary>
  public required string Reaction { get; init; }
}
