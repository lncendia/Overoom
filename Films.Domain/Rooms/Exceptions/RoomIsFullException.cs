namespace Films.Domain.Rooms.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда комната заполнена.
/// </summary>
public class RoomIsFullException : Exception
{
    /// <summary>
    /// Идентификатор комнаты, которая заполнена.
    /// </summary>
    public Guid RoomId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    public RoomIsFullException(Guid roomId) 
        : base($"Room with ID {roomId} is full and cannot accept more viewers.")
    {
        RoomId = roomId;
    }
}