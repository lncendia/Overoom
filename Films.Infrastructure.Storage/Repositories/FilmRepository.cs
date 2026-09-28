using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;
using Films.Domain.Films;
using Films.Domain.Films.Snapshots;
using Films.Domain.Films.Specifications.Visitor;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения фильмов.
/// </summary>
public class FilmRepository : RepositoryBase<FilmModel, Film, FilmSnapshot, IFilmSpecificationVisitor>, IFilmRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  public FilmRepository(MongoDbContext context, ModelBuilder config) : base(config, context.Films)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<FilmModel, bool>>? SpecificationVisitor(
    ISpecification<Film, IFilmSpecificationVisitor> spec)
  {
    var visitor = new FilmVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override FilmModel FactoryMethod(Film aggregate)
  {
    return new FilmModel
    {
      Id = aggregate.Id
    };
  }
}
