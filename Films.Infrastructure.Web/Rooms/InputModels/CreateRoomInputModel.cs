namespace Films.Infrastructure.Web.Rooms.InputModels;

/// <summary>
/// Модель данных для создания комнаты просмотра
/// </summary>
public class CreateRoomInputModel
{
    /// <summary>
    /// Признак открытой комнаты (доступной для всех)
    /// </summary>
    public bool Open { get; init; }

    /// <summary>
    /// Идентификатор фильма для просмотра (обязательное поле)
    /// </summary>
    public Guid FilmId { get; init; }
}