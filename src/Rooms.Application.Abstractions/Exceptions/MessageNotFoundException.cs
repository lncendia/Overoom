namespace Rooms.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда не удается найти сообщение.
/// </summary>
/// <param name="messageId">Идентификатор сообщения.</param>
public class MessageNotFoundException(Guid messageId) : Exception($"Message with ID {messageId} not found.")
{
  /// <summary>
  /// Идентификатор сообщения, которое не было найдено.
  /// </summary>
  public Guid MessageId { get; } = messageId;
}
