using Films.Application.Abstractions.DTOs.Common;
using MediatR;

namespace Films.Application.Abstractions.Commands.Films;

/// <summary>
/// Команда для изменения постера фильма
/// </summary>
public class ChangeFilmPosterCommand : IRequest
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// Файл постера
    /// </summary>
    public required FileDto Poster { get; init; }
}