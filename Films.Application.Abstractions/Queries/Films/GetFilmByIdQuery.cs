using Films.Application.Abstractions.DTOs.Films;
using MediatR;

namespace Films.Application.Abstractions.Queries.Films;

/// <summary>
/// Запрос для получения информации о фильме по идентификатору
/// </summary>
public class GetFilmByIdQuery : IRequest<FilmDto>
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя (опционально) для получения персональных данных
    /// </summary>
    public Guid? UserId { get; init; }
}