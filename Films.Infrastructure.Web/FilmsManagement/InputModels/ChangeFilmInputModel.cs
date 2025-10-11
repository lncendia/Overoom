namespace Films.Infrastructure.Web.FilmsManagement.InputModels;

/// <summary>
/// Модель данных для изменения фильма
/// </summary>
public class ChangeFilmInputModel
{
    /// <summary>
    /// Полное описание фильма (до 1500 символов)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Краткое описание фильма (до 500 символов)
    /// </summary>
    public string? ShortDescription { get; init; }

    /// <summary>
    /// Рейтинг Кинопоиска (0-10)
    /// </summary>
    public double? RatingKp { get; init; }

    /// <summary>
    /// Рейтинг IMDB (0-10)
    /// </summary>
    public double? RatingImdb { get; init; }
}