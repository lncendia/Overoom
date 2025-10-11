namespace Films.Infrastructure.Web.Rooms.InputModels;

/// <summary>
/// Модель данных для подключения к комнате
/// </summary>
public class JoinRoomInputModel
{
    /// <summary>
    /// Код доступа к комнате (не более 5 символов)
    /// </summary>
    public string? Code { get; init; }
}
