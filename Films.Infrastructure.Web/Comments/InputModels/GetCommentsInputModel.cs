using Films.Infrastructure.Web.Components.Interfaces;

namespace Films.Infrastructure.Web.Comments.InputModels;

/// <summary>
/// Модель данных для получения списка комментариев с пагинацией
/// </summary>
public class GetCommentsInputModel : IWithInputPagination
{
    /// <summary>
    /// Количество элементов для получения (по умолчанию 10, максимум 50)
    /// </summary>
    public int Take { get; init; } = 10;

    /// <summary>
    /// Количество элементов для пропуска
    /// </summary>
    public int Skip { get; init; } = 0;
}