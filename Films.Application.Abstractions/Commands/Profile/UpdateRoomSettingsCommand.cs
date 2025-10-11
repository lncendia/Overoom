using Common.Domain.Rooms;
using MediatR;

namespace Films.Application.Abstractions.Commands.Profile;

/// <summary>
/// Команда для обновления настроек комнаты пользователя
/// </summary>
public class UpdateRoomSettingsCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; set; }
    
    /// <summary>
    /// Настройки комнаты
    /// </summary>
    public required RoomSettings Settings { get; init; }
}