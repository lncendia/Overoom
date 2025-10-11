namespace Rooms.Application.Abstractions.DTOs;

/// <summary>
/// DTO для передачи данных комнаты
/// </summary>
public class RoomDto
{
    /// <summary>
    /// Уникальный идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор владельца комнаты
    /// </summary>
    public required Guid OwnerId { get; init; }
    
    /// <summary>
    /// Список зрителей в комнате
    /// </summary>
    public required IReadOnlyList<ViewerDto> Viewers { get; init; }
    
    /// <summary>
    /// Идентификатор фильма, который воспроизводится в комнате
    /// </summary>
    public required Guid FilmId { get; init; }
    
    /// <summary>
    /// Флаг, указывающий является ли контент сериалом
    /// </summary>
    public required bool IsSerial { get; init; }
}