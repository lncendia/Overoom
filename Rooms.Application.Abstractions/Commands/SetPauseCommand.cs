namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на установку паузы
/// </summary>
public class SetPauseCommand : RoomBaseCommand
{
    /// <summary>
    /// Таймлайн
    /// </summary>
    public required TimeSpan TimeLine { get; init; }
    
    /// <summary>
    /// Флаг нахождения на паузе
    /// </summary>
    public required bool Pause { get; init; }
    
    /// <summary>
    /// Флаг, что пауза вызвана дозагрузкой контента
    /// </summary>
    public required bool Buffering { get; init; }
}