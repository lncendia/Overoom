using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;
using Films.Domain.Playlists;
using Films.Domain.Playlists.Snapshots;
using Films.Domain.Playlists.Specifications.Visitor;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Playlists;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения подборок.
/// </summary>
public class PlaylistRepository : RepositoryBase<PlaylistModel, Playlist, PlaylistSnapshot, IPlaylistSpecificationVisitor>, IPlaylistRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  public PlaylistRepository(MongoDbContext context, ModelBuilder config) : base(config, context.Playlists)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<PlaylistModel, bool>>? SpecificationVisitor(
    ISpecification<Playlist, IPlaylistSpecificationVisitor> spec)
  {
    var visitor = new PlaylistVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override PlaylistModel FactoryMethod(Playlist aggregate)
  {
    return new PlaylistModel
    {
      Id = aggregate.Id,
      Name = null,
      Description = null,
      PosterKey = null
    };
  }
}
