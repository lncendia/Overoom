namespace Films.Infrastructure.Web.Films.InputModels;

/// <summary>
/// Модель данных для оценки фильма пользователем
/// </summary>
public class RateFilmInputModel
{
    /// <summary>
    /// Оценка фильма (должна быть в диапазоне от 0 до 10)
    /// </summary>
    public double Score { get; init; }
}
