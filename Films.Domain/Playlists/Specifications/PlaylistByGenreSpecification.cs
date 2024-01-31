using Films.Domain.Playlists.Entities;
using Films.Domain.Playlists.Specifications.Visitor;
using Films.Domain.Specifications.Abstractions;

namespace Films.Domain.Playlists.Specifications;

public class PlaylistByGenreSpecification(string genre) : ISpecification<Playlist, IPlaylistSpecificationVisitor>
{
    public string Genre { get; } = genre;

    public bool IsSatisfiedBy(Playlist item) =>
        item.Genres.Any(g => string.Equals(g, Genre, StringComparison.CurrentCultureIgnoreCase));

    public void Accept(IPlaylistSpecificationVisitor visitor) => visitor.Visit(this);
}