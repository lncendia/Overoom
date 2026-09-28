using Films.Domain.Films.ValueObjects;

namespace Films.Domain.Films.Snapshots;

public record FilmSnapshot
{
  public required Guid Id { get; init; }
  public required string Title { get; init; }
  public required string Description { get; init; }
  public required string ShortDescription { get; init; }
  public required DateOnly Date { get; init; }
  public required string PosterKey { get; init; }
  public required Rating? RatingKp { get; init; }
  public required Rating? RatingImdb { get; init; }
  public required MediaContent? Content { get; init; }
  public required IReadOnlyCollection<Season>? Seasons { get; init; }
  public required IReadOnlyCollection<string> Genres { get; init; }
  public required IReadOnlyCollection<string> Countries { get; init; }
  public required IReadOnlyCollection<Actor> Actors { get; init; }
  public required IReadOnlyCollection<string> Directors { get; init; }
  public required IReadOnlyCollection<string> Screenwriters { get; init; }
}
