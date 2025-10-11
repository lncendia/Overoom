namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на установку статуса подключения зрителя
/// </summary>
public class SetOnlineCommand : RoomBaseCommand
{
    public required bool Online { get; init; }
}