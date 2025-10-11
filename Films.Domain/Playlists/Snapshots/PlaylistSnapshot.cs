namespace Films.Domain.Playlists.Snapshots;

public class PlaylistSnapshot
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string PosterKey { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required IReadOnlyCollection<Guid> Films { get; init; }
    public required IReadOnlyCollection<string> Genres { get; init; }
}
