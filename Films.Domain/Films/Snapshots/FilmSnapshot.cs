using Films.Domain.Films.ValueObjects;

namespace Films.Domain.Films.Snapshots;

public record FilmSnapshot
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string ShortDescription { get; init; } = null!;
    public DateOnly Date { get; init; }
    public string PosterKey { get; init; } = null!;
    public Rating? RatingKp { get; init; }
    public Rating? RatingImdb { get; init; }
    public MediaContent? Content { get; init; }
    public IReadOnlySet<Season>? Seasons { get; init; }
    public IReadOnlyCollection<string> Genres { get; init; } = null!;
    public IReadOnlyCollection<string> Countries { get; init; } = null!;
    public IReadOnlyCollection<Actor> Actors { get; init; } = null!;
    public IReadOnlyCollection<string> Directors { get; init; } = null!;
    public IReadOnlyCollection<string> Screenwriters { get; init; } = null!;
}