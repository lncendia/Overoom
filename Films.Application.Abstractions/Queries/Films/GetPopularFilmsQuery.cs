using Films.Application.Abstractions.DTOs.Films;
using MediatR;

namespace Films.Application.Abstractions.Queries.Films;

/// <summary>
/// Запрос для получения популярных фильмов
/// </summary>
public class GetPopularFilmsQuery : IRequest<IReadOnlyList<FilmShortDto>>
{
    /// <summary>
    /// Количество получаемых фильмов
    /// </summary>
    public required int Take { get; init; }
}