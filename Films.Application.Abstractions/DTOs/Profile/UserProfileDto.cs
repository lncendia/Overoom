using Common.Domain.Rooms;

namespace Films.Application.Abstractions.DTOs.Profile;

/// <summary>
/// DTO для передачи данных профиля пользователя
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public required string UserName { get; init; }
    
    /// <summary>
    /// Ключ фотографии профиля в хранилище (может быть null)
    /// </summary>
    public string? PhotoKey { get; init; }
    
    /// <summary>
    /// Настройки комнаты пользователя
    /// </summary>
    public required RoomSettings RoomSettings { get; init; }
    
    /// <summary>
    /// Список предпочтительных жанров пользователя
    /// </summary>
    public required IReadOnlyList<string> Genres { get; init; }
}