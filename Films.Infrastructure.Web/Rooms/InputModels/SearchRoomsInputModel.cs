using Films.Infrastructure.Web.Components.Interfaces;

namespace Films.Infrastructure.Web.Rooms.InputModels;

/// <summary>
/// Модель данных для поиска комнат просмотра с фильтрами и пагинацией
/// </summary>
public class SearchRoomsInputModel : IWithInputPagination
{
    /// <summary>
    /// Идентификатор фильма для фильтрации комнат
    /// </summary>
    public Guid? FilmId { get; init; }

    /// <summary>
    /// Флаг поиска только публичных комнат
    /// </summary>
    public bool OnlyPublic { get; init; }

    /// <summary>
    /// Флаг поиска только созданных текущим пользователем комнат
    /// </summary>
    public bool OnlyMy { get; init; }

    /// <summary>
    /// Количество возвращаемых элементов (по умолчанию 10, максимум 50)
    /// </summary>
    public int Take { get; init; } = 10;

    /// <summary>
    /// Количество пропускаемых элементов
    /// </summary>
    public int Skip { get; init; } = 0;
}