namespace Films.Application.Abstractions.DTOs.Comments;

/// <summary>
/// DTO для передачи данных комментария
/// </summary>
public class CommentDto
{
    /// <summary>
    /// Уникальный идентификатор комментария
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя, оставившего комментарий
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Текст комментария
    /// </summary>
    public required string Text { get; init; }
    
    /// <summary>
    /// Дата и время создания комментария
    /// </summary>
    public required DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public required string UserName { get; init; }
    
    /// <summary>
    /// Ключ фотографии профиля пользователя (может быть null)
    /// </summary>
    public string? PhotoKey { get; init; }
}