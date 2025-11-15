namespace Films.Domain.Rooms.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда пользователь заблокирован в комнате.
/// </summary>
public class UserBannedInRoomException : Exception
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
    public UserBannedInRoomException(Guid userId, Guid roomId) 
        : base($"User with ID {userId} is banned in the room with ID {roomId}.")
    {
        UserId = userId;
        RoomId = roomId;
    }
}