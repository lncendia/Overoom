namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда не удается найти пользователя.
/// </summary>
public class UserNotFoundException : Exception
{
    /// <summary>
    /// Идентификатор пользователя, который не был найден.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    public UserNotFoundException(Guid userId) : base($"User with ID {userId} not found.")
    {
        UserId = userId;
    }
}