using MediatR;

namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда для изменения данных зрителя.
/// </summary>
public class ChangeViewersCommand : IRequest
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Ключ фотографии пользователя.
    /// </summary>
    public string? PhotoKey { get; init; }
}