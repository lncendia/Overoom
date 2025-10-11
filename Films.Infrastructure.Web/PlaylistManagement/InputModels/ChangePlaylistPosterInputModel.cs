namespace Films.Infrastructure.Web.PlaylistManagement.InputModels;

/// <summary>
/// Модель данных для изменения постера подборки
/// </summary>
public class ChangePlaylistPosterInputModel
{
    /// <summary>
    /// Постер подборки
    /// </summary>
    public IFormFile? Poster { get; init; }
}