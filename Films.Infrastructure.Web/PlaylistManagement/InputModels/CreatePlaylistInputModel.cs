namespace Films.Infrastructure.Web.PlaylistManagement.InputModels;

/// <summary>
/// Модель данных для создания плейлиста
/// </summary>
public class CreatePlaylistInputModel
{
    /// <summary>
    /// Название плейлиста (обязательное, не более 200 символов)
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Описание плейлиста (обязательное, не более 500 символов)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Список идентификаторов фильмов в плейлисте
    /// </summary>
    public Guid[] Films { get; init; } = [];
}