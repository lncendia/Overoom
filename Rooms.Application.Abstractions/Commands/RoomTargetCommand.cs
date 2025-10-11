namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Базовый класс для команды комнаты направленной на какого-то зрителя
/// </summary>
public abstract class RoomTargetCommand : RoomBaseCommand
{
    public required Guid TargetId { get; init; }
}