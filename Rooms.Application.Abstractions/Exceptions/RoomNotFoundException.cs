namespace Rooms.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда не удается найти комнату.
/// </summary>
public class RoomNotFoundException : Exception
{
    /// <summary>
    /// Идентификатор комнаты, которая не была найдена.
    /// </summary>
    public Guid RoomId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    public RoomNotFoundException(Guid roomId) : base($"Room with ID {roomId} not found.")
    {
        RoomId = roomId;
    }
}