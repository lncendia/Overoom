namespace Films.Domain.Rooms.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда предоставлен неверный код доступа.
/// </summary>
public class InvalidCodeException : Exception
{
    /// <summary>
    /// Идентификатор комнаты, для которой код неверный.
    /// </summary>
    public Guid RoomId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    public InvalidCodeException(Guid roomId) 
        : base($"The provided code is invalid for room with ID {roomId}.")
    {
        RoomId = roomId;
    }
}