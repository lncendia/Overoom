namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда достигнуто максимальное количество комнат.
/// </summary>
public class MaxNumberRoomsReachedException : Exception
{
    /// <summary>
    /// Идентификатор пользователя, который пытался создать комнату.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    public MaxNumberRoomsReachedException(Guid userId) 
        : base($"User with ID {userId} has reached the maximum number of rooms.")
    {
        UserId = userId;
    }
}