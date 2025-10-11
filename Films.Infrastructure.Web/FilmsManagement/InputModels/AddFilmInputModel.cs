namespace Films.Infrastructure.Web.FilmsManagement.InputModels;

/// <summary>
/// Модель данных для добавления нового фильма
/// </summary>
public class AddFilmInputModel
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
    /// Название фильма (до 200 символов)
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Дата выпуска
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Рейтинг Кинопоиска (0-10)
    /// </summary>
    public double? RatingKp { get; init; }

    /// <summary>
    /// Рейтинг IMDB (0-10)
    /// </summary>
    public double? RatingImdb { get; init; }

    /// <summary>
    /// Страны производства
    /// </summary>
    public string[] Countries { get; init; } = [];

    /// <summary>
    /// Актёры
    /// </summary>
    public ActorInputModel[] Actors { get; init; } = [];

    /// <summary>
    /// Режиссёры
    /// </summary>
    public string[] Directors { get; init; } = [];

    /// <summary>
    /// Жанры
    /// </summary>
    public string[] Genres { get; init; } = [];

    /// <summary>
    /// Сценаристы
    /// </summary>
    public string[] Screenwriters { get; init; } = [];
}
