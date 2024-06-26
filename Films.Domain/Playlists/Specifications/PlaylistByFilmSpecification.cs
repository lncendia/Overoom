using Films.Domain.Playlists.Specifications.Visitor;
using Films.Domain.Specifications.Abstractions;

namespace Films.Domain.Playlists.Specifications;

public class PlaylistByFilmSpecification(Guid id) : ISpecification<Playlist, IPlaylistSpecificationVisitor>
{
    public Guid Id { get; } = id;
    public bool IsSatisfiedBy(Playlist item) => item.Films.Any(f => f == Id);

    public void Accept(IPlaylistSpecificationVisitor visitor) => visitor.Visit(this);
}