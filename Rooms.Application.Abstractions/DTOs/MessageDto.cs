namespace Rooms.Application.Abstractions.DTOs;

/// <summary>
/// DTO для передачи данных сообщения в чате
/// </summary>
public class MessageDto
{
    /// <summary>
    /// Уникальный идентификатор сообщения
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя, отправившего сообщение
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Текст сообщения
    /// </summary>
    public required string Text { get; init; }
    
    /// <summary>
    /// Дата и время отправки сообщения
    /// </summary>
    public required DateTime SentAt { get; init; }
}