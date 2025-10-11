using Common.Domain.Rooms;
using MediatR;

namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда для изменения данных зрителя.
/// </summary>
public class ChangeViewersSettingsCommand : IRequest
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Настройки пользователя.
    /// </summary>
    public required RoomSettings Settings { get; init; }
}