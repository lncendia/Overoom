namespace Films.Infrastructure.Web.FilmsManagement.InputModels;

/// <summary>
/// Модель данных для изменения постера фильма
/// </summary>
public class ChangeFilmPosterInputModel
{
    /// <summary>
    /// Постер фильма
    /// </summary>
    public IFormFile? Poster { get; init; }
}