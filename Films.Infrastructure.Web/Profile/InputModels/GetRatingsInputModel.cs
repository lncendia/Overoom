using Films.Infrastructure.Web.Components.Interfaces;

namespace Films.Infrastructure.Web.Profile.InputModels;

/// <summary>
/// Модель данных для получения рейтингов с пагинацией
/// </summary>
public class GetRatingsInputModel : IWithInputPagination
{
    /// <summary>
    /// Количество возвращаемых элементов (по умолчанию 10, максимум 50)
    /// </summary>
    public int Take { get; init; } = 10;

    /// <summary>
    /// Количество пропускаемых элементов
    /// </summary>
    public int Skip { get; init; } = 0;
}