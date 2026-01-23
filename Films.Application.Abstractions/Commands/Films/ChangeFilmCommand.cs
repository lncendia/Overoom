using MediatR;

namespace Films.Application.Abstractions.Commands.Films;

/// <summary>
/// Команда для изменения информации о фильме
/// </summary>
public class ChangeFilmCommand : IRequest
{
  /// <summary>
  /// Идентификатор фильма
  /// </summary>
  public required Guid Id { get; set; }

  /// <summary>
  /// Полное описание фильма
  /// </summary>
  public required string Description { get; init; }

  /// <summary>
  /// Краткое описание фильма
  /// </summary>
  public string? ShortDescription { get; init; }

  /// <summary>
  /// Рейтинг КиноПоиска
  /// </summary>
  public double? RatingKp { get; init; }

  /// <summary>
  /// Рейтинг IMDb
  /// </summary>
  public double? RatingImdb { get; init; }
}
