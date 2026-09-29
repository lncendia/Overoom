using Common.Domain.Specifications.Abstractions;

namespace Films.Domain.Playlists.Specifications.Visitor;

public interface IPlaylistSpecificationVisitor : ISpecificationVisitor<IPlaylistSpecificationVisitor, Playlist>
{
  void Visit(DuplicatePlaylistsSpecification specification);
}