using MediatR;

namespace Films.Application.Abstractions.Queries.Rooms;

/// <summary>
/// Запрос для получения кода подключения к комнате по идентификатору
/// </summary>
public class GetRoomCodeQuery : IRequest<string?>
{
    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя для получения персональных данных
    /// </summary>
    public required Guid UserId { get; init; }
}