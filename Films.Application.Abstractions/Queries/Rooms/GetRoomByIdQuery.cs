using Films.Application.Abstractions.DTOs.Rooms;
using MediatR;

namespace Films.Application.Abstractions.Queries.Rooms;

/// <summary>
/// Запрос для получения информации о комнате по идентификатору
/// </summary>
public class GetRoomByIdQuery : IRequest<RoomDto>
{
    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя (опционально) для получения персональных данных
    /// </summary>
    public Guid? UserId { get; init; }
}