namespace Common.IntegrationEvents.Users;

/// <summary>
/// Событие интеграции, которое генерируется при изменении данных пользователя.
/// </summary>
public class UserInfoChangedIntegrationEvent
{
    /// <summary>
    /// Идентификатор пользователя, данные которого были изменены.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Ключ фотографии пользователя.
    /// </summary>
    public string? PhotoKey { get; init; }

    /// <summary>
    /// Новое имя пользователя.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Новая электронная почта пользователя.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Новая локаль пользователя.
    /// </summary>
    public required string Locale { get; init; }
}