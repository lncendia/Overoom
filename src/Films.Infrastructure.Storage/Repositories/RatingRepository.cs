using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;
using Common.Infrastructure.Transactions;

using Films.Domain.Ratings;
using Films.Domain.Ratings.Snapshots;
using Films.Domain.Ratings.Specifications.Visitor;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Ratings;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения оценок.
/// </summary>
public class RatingRepository : RepositoryBase<RatingModel, Rating, RatingSnapshot, IRatingSpecificationVisitor>, IRatingRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  /// <param name="transaction">Текущая транзакция области</param>
  public RatingRepository(MongoDbContext context, ModelBuilder config, ITransactionContext transaction)
    : base(config, context.Ratings, transaction)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<RatingModel, bool>>? SpecificationVisitor(
    ISpecification<Rating, IRatingSpecificationVisitor> spec)
  {
    var visitor = new RatingVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override RatingModel FactoryMethod(Rating aggregate)
  {
    return new RatingModel
    {
      Id = aggregate.Id
    };
  }
}
