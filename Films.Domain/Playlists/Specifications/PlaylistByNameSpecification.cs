using Common.Domain.Specifications.Abstractions;
using Films.Domain.Playlists.Specifications.Visitor;

namespace Films.Domain.Playlists.Specifications;

public class PlaylistByNameSpecification(string name) : ISpecification<Playlist, IPlaylistSpecificationVisitor>
{
    public string Name { get; } = name;
    public bool IsSatisfiedBy(Playlist item) => item.Name.Contains(Name);

    public void Accept(IPlaylistSpecificationVisitor visitor) => visitor.Visit(this);
}