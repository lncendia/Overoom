namespace Rooms.Domain.Rooms.Events;

/// <summary>
/// Интерфейс-маркер для событий, связанных с воспроизведением медиа
/// </summary>
public interface IPlayerEvent
{
    /// <summary>
    /// Флаг, указывающий является ли событие результатом синхронизации
    /// (а не ручного действия пользователя)
    /// </summary>
    bool IsSyncEvent { get; init; }
}