using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;

using Films.Domain.Repositories;
using Films.Domain.Rooms;
using Films.Domain.Rooms.Snapshots;
using Films.Domain.Rooms.Specifications.Visitor;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Rooms;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения комнат.
/// </summary>
public class RoomRepository : RepositoryBase<RoomModel, Room, RoomSnapshot, IRoomSpecificationVisitor>, IRoomRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  public RoomRepository(MongoDbContext context, ModelBuilder config) : base(config, context.Rooms)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<RoomModel, bool>>? SpecificationVisitor(ISpecification<Room, IRoomSpecificationVisitor> spec)
  {
    var visitor = new RoomVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override RoomModel FactoryMethod(Room aggregate)
  {
    return new RoomModel
    {
      Id = aggregate.Id
    };
  }

  /// <inheritdoc/>
  protected override Room FromSnapshot(RoomSnapshot snapshot)
  {
    return Room.FromSnapshot(snapshot);
  }
}
