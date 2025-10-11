using Common.Domain.Rooms;

namespace Rooms.Application.Abstractions.DTOs;

/// <summary>
/// DTO для передачи основных данных зрителя
/// </summary>
public class ViewerData
{
    /// <summary>
    /// Уникальный идентификатор зрителя
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Имя пользователя зрителя
    /// </summary>
    public required string UserName { get; init; }
    
    /// <summary>
    /// Ключ фотографии профиля зрителя (может быть null)
    /// </summary>
    public string? PhotoKey { get; init; }
    
    /// <summary>
    /// Настройки зрителя в комнате
    /// </summary>
    public required RoomSettings Settings { get; init; }
}