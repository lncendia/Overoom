using MediatR;

namespace Films.Application.Abstractions.Commands.Profile;

/// <summary>
/// Команда для добавления пользователя.
/// </summary>
public class AddUserCommand : IRequest
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Ключ фотографии пользователя.
    /// </summary>
    public string? PhotoKey { get; init; }
}