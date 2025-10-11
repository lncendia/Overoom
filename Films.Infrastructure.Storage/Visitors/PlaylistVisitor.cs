using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Films.Domain.Playlists;
using Films.Domain.Playlists.Specifications;
using Films.Domain.Playlists.Specifications.Visitor;
using Films.Infrastructure.Storage.Models.Playlists;

namespace Films.Infrastructure.Storage.Visitors;

public class PlaylistVisitor : BaseSpecificationVisitor<PlaylistModel, IPlaylistSpecificationVisitor, Playlist>,
    IPlaylistSpecificationVisitor
{
    protected override Expression<Func<PlaylistModel, bool>> ConvertSpecToExpression(
        ISpecification<Playlist, IPlaylistSpecificationVisitor> spec)
    {
        var visitor = new PlaylistVisitor();
        spec.Accept(visitor);
        return visitor.Expr!;
    }

    public void Visit(PlaylistByNameSpecification specification) =>
        Expr = model => model.Name.Contains(specification.Name);
}