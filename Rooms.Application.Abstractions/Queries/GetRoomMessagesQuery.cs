using Common.Application.DTOs;
using MediatR;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.Queries;

/// <summary>
/// Запрос для получения сообщений комнаты с фильмом.
/// </summary>
public class GetRoomMessagesQuery : IRequest<CountResult<MessageDto>>
{
    /// <summary>
    /// Идентификатор комнаты.
    /// </summary>
    public required Guid RoomId { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid ViewerId { get; init; }

    /// <summary>
    /// Идентификатор сообщения, с которого начинать выборку.
    /// </summary>
    public Guid? FromMessageId { get; init; }

    /// <summary>
    /// Количество сообщений для выборки.
    /// </summary>
    public required int Count { get; init; }
}