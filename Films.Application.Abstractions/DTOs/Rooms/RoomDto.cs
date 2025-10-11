namespace Films.Application.Abstractions.DTOs.Rooms;

/// <summary>
/// DTO для передачи основных данных комнаты
/// </summary>
public class RoomDto
{
    /// <summary>
    /// Уникальный идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор фильма, который воспроизводится в комнате
    /// </summary>
    public required Guid FilmId { get; init; }
    
    /// <summary>
    /// Количество зрителей в комнате
    /// </summary>
    public required int ViewersCount { get; init; }
    
    /// <summary>
    /// Флаг, указывающий является ли комната приватной
    /// </summary>
    public required bool IsPrivate { get; init; }
    
    /// <summary>
    /// Флаг, указывающий находится ли текущий пользователь в комнате
    /// </summary>
    public required bool IsUserIn { get; init; }
}