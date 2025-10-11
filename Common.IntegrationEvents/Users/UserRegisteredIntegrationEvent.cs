namespace Common.IntegrationEvents.Users;

/// <summary>
/// Событие интеграции, которое генерируется при создании нового пользователя.
/// </summary>
public class UserRegisteredIntegrationEvent
{
    /// <summary>
    /// Идентификатор созданного пользователя.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Ключ фотографии пользователя.
    /// </summary>
    public string? PhotoKey { get; init; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Локаль пользователя.
    /// </summary>
    public required string Locale { get; init; }
    
    /// <summary>
    /// Время регистрации пользователя.
    /// </summary>
    public required DateTime RegistrationTimeUtc { get; init; }
}