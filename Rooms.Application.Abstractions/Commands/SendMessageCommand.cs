using MediatR;

namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на отправку сообщения
/// </summary>
public class SendMessageCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid ViewerId { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public required string ConnectionId { get; init; }

    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
    
    /// <summary>
    /// Текст сообщения
    /// </summary>
    public required string Message { get; init; }
}