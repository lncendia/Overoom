using Films.Application.Abstractions.DTOs.Films;
using MediatR;

namespace Films.Application.Abstractions.Queries.Profile;

/// <summary>
/// Запрос для получения истории просмотров пользователя
/// </summary>
public class GetUserHistoryQuery : IRequest<IReadOnlyList<FilmShortDto>>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid Id { get; init; }
}