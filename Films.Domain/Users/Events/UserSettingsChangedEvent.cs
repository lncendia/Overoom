using Common.Domain.Events;

namespace Films.Domain.Users.Events;

/// <summary>
/// Событие, представляющее изменение настроек пользователя в системе
/// </summary>
public class UserSettingsChangedEvent(User user) : DomainEvent
{
    /// <summary>
    /// Пользователь, чьи настройки были изменены
    /// </summary>
    public User User { get; } = user;
}