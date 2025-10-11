using Films.Domain.Films.ValueObjects;
using MediatR;

namespace Films.Application.Abstractions.Commands.Films;

/// <summary>
/// Команда для добавления нового фильма
/// </summary>
public class AddFilmCommand : IRequest<Guid>
{
    /// <summary>
    /// Полное описание фильма
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Краткое описание фильма (может быть null)
    /// </summary>
    public string? ShortDescription { get; init; }
    
    /// <summary>
    /// Название фильма
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// Дата выпуска фильма
    /// </summary>
    public required DateOnly Date { get; init; }
    
    /// <summary>
    /// Рейтинг КиноПоиска (может быть null)
    /// </summary>
    public double? RatingKp { get; init; }
    
    /// <summary>
    /// Рейтинг IMDb (может быть null)
    /// </summary>
    public double? RatingImdb { get; init; }
    
    /// <summary>
    /// Список стран производства
    /// </summary>
    public required IReadOnlyList<string> Countries { get; init; }
    
    /// <summary>
    /// Список актеров
    /// </summary>
    public required IReadOnlyList<Actor> Actors { get; init; }
    
    /// <summary>
    /// Список режиссеров
    /// </summary>
    public required IReadOnlyList<string> Directors { get; init; }
    
    /// <summary>
    /// Список жанров
    /// </summary>
    public required IReadOnlyList<string> Genres { get; init; }
    
    /// <summary>
    /// Список сценаристов
    /// </summary>
    public required IReadOnlyList<string> Screenwriters { get; init; }
}