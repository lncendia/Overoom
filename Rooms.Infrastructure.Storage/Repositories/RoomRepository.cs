using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;

using Incendia.MongoTracker.Builders;

using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Snapshots;
using Rooms.Domain.Rooms.Specifications.Visitor;
using Rooms.Infrastructure.Storage.Context;
using Rooms.Infrastructure.Storage.Models.Rooms;
using Rooms.Infrastructure.Storage.Visitors;

namespace Rooms.Infrastructure.Storage.Repositories;

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
}
