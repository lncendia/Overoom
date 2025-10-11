using Films.Application.Abstractions.DTOs.Films;
using MediatR;

namespace Films.Application.Abstractions.Queries.Profile;

/// <summary>
/// Запрос для получения списка ожидания пользователя
/// </summary>
public class GetUserWatchlistQuery : IRequest<IReadOnlyList<FilmShortDto>>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid Id { get; init; }
}