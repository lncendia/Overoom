namespace Films.Domain.Rooms.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда пользователь уже состоит в комнате.
/// </summary>
public class UserAlreadyInRoomException : Exception
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Идентификатор комнаты.
    /// </summary>
    public Guid RoomId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="roomId">Идентификатор комнаты.</param>
    public UserAlreadyInRoomException(Guid userId, Guid roomId) 
        : base($"User with ID {userId} is already joined in the room with ID {roomId}.")
    {
        UserId = userId;
        RoomId = roomId;
    }
}