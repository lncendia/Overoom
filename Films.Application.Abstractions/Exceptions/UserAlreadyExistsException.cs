namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда пользователь уже существует.
/// </summary>
public class UserAlreadyExistsException : Exception
{
    /// <summary>
    /// Идентификатор пользователя, который уже существует.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    public UserAlreadyExistsException(Guid userId) 
        : base($"User with ID {userId} already exists.")
    {
        UserId = userId;
    }
}