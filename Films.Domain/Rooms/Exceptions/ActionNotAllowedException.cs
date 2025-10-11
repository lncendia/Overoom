namespace Films.Domain.Rooms.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда действие не разрешено пользователю.
/// </summary>
public class ActionNotAllowedException : Exception
{
    /// <summary>
    /// Идентификатор комнаты, в которой действие запрещено.
    /// </summary>
    public Guid RoomId { get; }

    /// <summary>
    /// Действие, которое не разрешено пользователю.
    /// </summary>
    public string Action { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="action">Действие, которое не разрешено.</param>
    public ActionNotAllowedException(Guid roomId, string action) 
        : base($"Action '{action}' is not allowed in room with ID {roomId}.")
    {
        RoomId = roomId;
        Action = action;
    }
}