namespace Films.Infrastructure.Web.PlaylistManagement.InputModels;

/// <summary>
/// Модель данных для изменения плейлиста
/// </summary>
public class ChangePlaylistInputModel
{
    /// <summary>
    /// Новое описание плейлиста (не более 500 символов)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Список идентификаторов фильмов в плейлисте
    /// </summary>
    public Guid[]? Films { get; init; }
}
