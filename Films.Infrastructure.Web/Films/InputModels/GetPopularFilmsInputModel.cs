namespace Films.Infrastructure.Web.Films.InputModels;

/// <summary>
/// Модель данных для получения списка популярных фильмов
/// </summary>
public class GetPopularFilmsInputModel
{
    /// <summary>
    /// Количество возвращаемых фильмов (по умолчанию 15, максимум 30)
    /// </summary>
    public int Take { get; init; } = 15;
}