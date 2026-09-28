using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;

using Films.Domain.Ratings;
using Films.Domain.Ratings.Snapshots;
using Films.Domain.Ratings.Specifications.Visitor;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Models.Ratings;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

using MongoDB.Driver;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения оценок.
/// </summary>
/// <remarks>
/// Помимо самих оценок поддерживает денормализованные счётчики оценок в документах фильмов:
/// изменения накапливаются до сохранения и применяются атомарным $inc в той же сессии.
/// </remarks>
public class RatingRepository : RepositoryBase<RatingModel, Rating, RatingSnapshot, IRatingSpecificationVisitor>, IRatingRepository
{
  /// <summary>
  /// Коллекция фильмов, в которой хранятся счётчики оценок
  /// </summary>
  private readonly IMongoCollection<FilmModel> _films;

  /// <summary>
  /// Накопленные изменения счётчиков по фильмам: количество и сумма оценок
  /// </summary>
  private readonly Dictionary<Guid, (int Count, double Sum)> _filmRatingDeltas = [];

  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  public RatingRepository(MongoDbContext context, ModelBuilder config) : base(config, context.Ratings)
  {
    _films = context.Films;
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

  /// <inheritdoc/>
  protected override void OnModelAdded(RatingModel model)
  {
    AddDelta(model.FilmId, 1, model.Score);
  }

  /// <inheritdoc/>
  protected override void OnModelUpdating(RatingModel model, RatingSnapshot snapshot)
  {
    AddDelta(model.FilmId, 0, snapshot.Score - model.Score);
  }

  /// <inheritdoc/>
  protected override void OnModelDeleted(RatingModel model)
  {
    AddDelta(model.FilmId, -1, -model.Score);
  }

  /// <inheritdoc/>
  protected override async Task OnCommittedAsync(IClientSessionHandle sessionHandle, CancellationToken token)
  {
    UpdateOneModel<FilmModel>[] updates = _filmRatingDeltas
      .Where(d => d.Value.Count != 0 || d.Value.Sum != 0)
      .Select(d => new UpdateOneModel<FilmModel>(
        Builders<FilmModel>.Filter.Eq(f => f.Id, d.Key),
        Builders<FilmModel>.Update
          .Inc(f => f.UserRatingsCount, d.Value.Count)
          .Inc(f => f.UserRatingsSum, d.Value.Sum)))
      .ToArray();

    if (updates.Length > 0)
      await _films.BulkWriteAsync(sessionHandle, updates, new BulkWriteOptions { IsOrdered = false }, token);

    _filmRatingDeltas.Clear();
  }

  /// <summary>
  /// Добавляет изменение к накопленным счётчикам фильма
  /// </summary>
  /// <param name="filmId">Идентификатор фильма</param>
  /// <param name="count">Изменение количества оценок</param>
  /// <param name="sum">Изменение суммы оценок</param>
  private void AddDelta(Guid filmId, int count, double sum)
  {
    (int Count, double Sum) current = _filmRatingDeltas.GetValueOrDefault(filmId);
    _filmRatingDeltas[filmId] = (current.Count + count, current.Sum + sum);
  }
}
