using System.Linq.Expressions;

using Common.Domain.Aggregates;
using Common.Domain.Events;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Models;

using Incendia.MongoTracker.Builders;
using Incendia.MongoTracker.Tracker;

using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Common.Infrastructure.Repositories;

/// <summary>
/// Базовый класс репозитория для работы с MongoDB
/// </summary>
/// <typeparam name="T">Тип сущности, наследуемый от UpdatedEntity</typeparam>
/// <typeparam name="TA">Тип агрегата</typeparam>
/// <typeparam name="TV">Тип посетителя спецификаций для агрегата</typeparam>
/// <typeparam name="TS">Тип снапшота агрегата</typeparam>
public abstract class RepositoryBase<T, TA, TS, TV> : MongoTracker<T>, IRepository
  where T : class, IModel<TS>
  where TA : AggregateRoot, ISnapshotable<TA, TS>
  where TV : ISpecificationVisitor<TV, TA>
{
  #region Поля и свойства

  /// <summary>
  /// Коллекция доменных событий, связанных с изменениями в репозитории
  /// </summary>
  private readonly HashSet<DomainEvent> _events = [];

  /// <summary>
  /// Коллекция MongoDB, с которой работает репозиторий
  /// </summary>
  private IMongoCollection<T> Collection { get; }

  #endregion

  #region IRepository

  /// <summary>
  /// Доступ только для чтения к коллекции доменных событий
  /// </summary>
  public IReadOnlySet<DomainEvent> Events => _events;

  /// <summary>
  /// Применяет все изменения в репозитории к базе данных в рамках текущей сессии
  /// </summary>
  /// <param name="sessionHandle">Сессия MongoDB</param>
  /// <param name="token">Токен отмены операции</param>
  public async Task CommitAsync(IClientSessionHandle sessionHandle, CancellationToken token)
  {
    await SaveChangesAsync(Collection, sessionHandle, cancellationToken: token);
    await OnCommittedAsync(sessionHandle, token);
  }

  #endregion

  #region Методы

  /// <summary>
  /// Асинхронно добавляет новый агрегат.
  /// </summary>
  /// <param name="aggregate">Добавляемый агрегат</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public Task AddAsync(TA aggregate, CancellationToken cancellationToken = default)
  {
    AddAggregate(aggregate);
    return Task.CompletedTask;
  }

  /// <summary>
  /// Асинхронно добавляет новые агрегаты.
  /// </summary>
  /// <param name="aggregates">Коллекция добавляемых агрегатов</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public Task AddRangeAsync(IReadOnlyList<TA> aggregates, CancellationToken cancellationToken = default)
  {
    foreach (TA aggregate in aggregates)
    {
      AddAggregate(aggregate);
    }

    return Task.CompletedTask;
  }

  /// <summary>
  /// Асинхронно обновляет информацию об агрегате.
  /// </summary>
  /// <param name="aggregate">Обновляемый агрегат</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public Task UpdateAsync(TA aggregate, CancellationToken cancellationToken = default)
  {
    UpdateAggregate(aggregate);
    return Task.CompletedTask;
  }

  /// <summary>
  /// Асинхронно обновляет информацию об агрегатах.
  /// </summary>
  /// <param name="aggregates">Коллекция обновляемых агрегатов</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public Task UpdateRangeAsync(IReadOnlyList<TA> aggregates, CancellationToken cancellationToken = default)
  {
    foreach (TA aggregate in aggregates)
    {
      UpdateAggregate(aggregate);
    }

    return Task.CompletedTask;
  }

  /// <summary>
  /// Асинхронно удаляет агрегат по ее ключу.
  /// </summary>
  /// <param name="aggregate">Удаляемый агрегат</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public Task DeleteAsync(TA aggregate, CancellationToken cancellationToken = default)
  {
    T model = Get(aggregate.Id);
    Delete(model, model.Id);
    return Task.CompletedTask;
  }

  /// <summary>
  /// Асинхронно удаляет агрегаты по спецификации.
  /// </summary>
  /// <param name="specification">Спецификация для выбора агрегатов</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Количество удалённых агрегатов</returns>
  public async Task<int> DeleteRangeAsync(ISpecification<TA, TV> specification,
    CancellationToken cancellationToken = default)
  {
    IQueryable<T>? query = Collection.AsQueryable();

    Expression<Func<T, bool>>? expression = SpecificationVisitor(specification);
    if (expression != null) query = query.Where(expression);

    List<T>? entities = await query.ToListAsync(cancellationToken: cancellationToken);

    foreach (T trackedEntity in entities.Select(Track))
    {
      Delete(trackedEntity, trackedEntity.Id);
    }

    return entities.Count;
  }

  /// <summary>
  /// Асинхронно получает агрегат по идентификатору.
  /// </summary>
  /// <param name="id">Идентификатор агрегата</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Агрегат или null, если не найден</returns>
  public async Task<TA?> GetAsync(Guid id, CancellationToken cancellationToken = default)
  {
    T? model = await Collection.AsQueryable()
      .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    if (model == null) return null;
    model = Track(model);
    return TA.Restore(model.GetSnapshot());
  }

  /// <summary>
  /// Асинхронно выполняет поиск агрегатов, удовлетворяющих указанной спецификации, с возможностью сортировки, пропуска и взятия определенного количества.
  /// </summary>
  /// <param name="specification">Спецификация для фильтрации агрегатов</param>
  /// <param name="skip">Количество пропускаемых элементов</param>
  /// <param name="take">Максимальное количество возвращаемых элементов</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Коллекция найденных агрегатов</returns>
  public async Task<IReadOnlyList<TA>> FindAsync(
    ISpecification<TA, TV>? specification,
    int? skip = null, int? take = null,
    CancellationToken cancellationToken = default)
  {
    IQueryable<T>? query = Collection.AsQueryable();

    if (specification != null)
    {
      Expression<Func<T, bool>>? expression = SpecificationVisitor(specification);
      if (expression != null) query = query.Where(expression);
    }

    query = query.OrderBy(i => i.Id);
    if (skip.HasValue) query = query.Skip(skip.Value);
    if (take.HasValue) query = query.Take(take.Value);

    List<T>? models = await query.ToListAsync(cancellationToken: cancellationToken);

    return
    [
      .. models
        .Select(Track)
        .Select(m => TA.Restore(m.GetSnapshot()))
    ];
  }

  /// <summary>
  /// Асинхронно возвращает первый агрегат, удовлетворяющих указанной спецификации или значение по умолчанию.
  /// </summary>
  /// <param name="specification">Спецификация для фильтрации агрегатов</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Первый найденный агрегат или null</returns>
  public async Task<TA?> FirstOrDefaultAsync(ISpecification<TA, TV>? specification,
    CancellationToken cancellationToken = default)
  {
    IReadOnlyList<TA> results = await FindAsync(
      specification: specification,
      skip: 0,
      take: 1,
      cancellationToken: cancellationToken);

    return results.FirstOrDefault();
  }

  /// <summary>
  /// Возвращает количество агрегатов, удовлетворяющих указанной спецификации.
  /// </summary>
  /// <param name="specification">Спецификация для фильтрации агрегатов</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Количество агрегатов</returns>
  public async Task<int> CountAsync(ISpecification<TA, TV>? specification,
    CancellationToken cancellationToken = default)
  {
    IQueryable<T>? query = Collection.AsQueryable();

    if (specification == null) return await query.CountAsync(cancellationToken: cancellationToken);

    Expression<Func<T, bool>>? expression = SpecificationVisitor(specification);

    if (expression != null) query = query.Where(expression);
    return await query.CountAsync(cancellationToken: cancellationToken);
  }

  /// <inheritdoc cref="IRepository"/>
  /// <summary>
  /// Добавляет доменные события в коллекцию репозитория
  /// </summary>
  /// <param name="aggregate">Агрегат для обновления.</param>
  private void AddEvents(TA aggregate)
  {
    foreach (DomainEvent @event in aggregate.DomainEvents)
    {
      _events.Add(@event);
    }
  }

  /// <summary>
  /// Создаёт модель для нового агрегата и начинает её отслеживать
  /// </summary>
  /// <param name="aggregate">Добавляемый агрегат</param>
  private void AddAggregate(TA aggregate)
  {
    T model = FactoryMethod(aggregate);
    model.UpdateFromSnapshot(aggregate.ToSnapshot());
    OnModelAdded(model);
    Add(model, aggregate);
  }

  /// <summary>
  /// Переносит состояние агрегата в отслеживаемую модель
  /// </summary>
  /// <param name="aggregate">Обновляемый агрегат</param>
  private void UpdateAggregate(TA aggregate)
  {
    T model = Get(aggregate.Id);
    TS snapshot = aggregate.ToSnapshot();
    OnModelUpdating(model, snapshot);
    model.UpdateFromSnapshot(snapshot);
    Update(aggregate);
  }

  /// <summary>
  /// Добавляет сущность в репозиторий и регистрирует событие создания агрегата
  /// </summary>
  /// <param name="entity">Сущность для добавления в репозиторий</param>
  /// <param name="aggregate">Агрегат, связанный с добавляемой сущностью</param>
  private void Add(T entity, TA aggregate)
  {
    Add(entity);
    AddEvents(aggregate);

    _events.Add(new CreateEvent<TA> { Aggregate = aggregate });
    _events.Add(new SaveEvent<TA> { Aggregate = aggregate });
  }

  /// <summary>
  /// Регистрирует событие обновления агрегата
  /// </summary>
  /// <param name="aggregate">Обновленный агрегат</param>
  private void Update(TA aggregate)
  {
    AddEvents(aggregate);
    _events.Add(new SaveEvent<TA> { Aggregate = aggregate });
  }

  /// <summary>
  /// Удаляет сущность из репозитория и регистрирует событие удаления агрегата
  /// </summary>
  /// <param name="entity">Сущность, которую необходимо удалить</param>
  /// <param name="id">Идентификатор сущности для удаления</param>
  private void Delete(T entity, Guid id)
  {
    OnModelDeleted(entity);
    Delete(entity);
    _events.Add(new DeleteEvent<TA> { Id = id });
  }

  /// <summary>
  /// Преобразует спецификацию агрегата в LINQ-выражение для модели MongoDB.
  /// </summary>
  /// <param name="spec">Спецификация агрегата</param>
  /// <returns>Выражение фильтрации или null</returns>
  protected abstract Expression<Func<T, bool>>? SpecificationVisitor(ISpecification<TA, TV> spec);

  /// <summary>
  /// Создаёт модель MongoDB на основе агрегата.
  /// </summary>
  /// <param name="aggregate">Исходный агрегат</param>
  /// <returns>Модель для сохранения в MongoDB</returns>
  protected abstract T FactoryMethod(TA aggregate);

  /// <summary>
  /// Вызывается после создания модели нового агрегата, до сохранения.
  /// </summary>
  /// <param name="model">Модель нового агрегата</param>
  protected virtual void OnModelAdded(T model)
  {
  }

  /// <summary>
  /// Вызывается перед переносом снапшота в модель, пока модель ещё хранит предыдущее состояние.
  /// </summary>
  /// <param name="model">Модель с предыдущим состоянием</param>
  /// <param name="snapshot">Новое состояние агрегата</param>
  protected virtual void OnModelUpdating(T model, TS snapshot)
  {
  }

  /// <summary>
  /// Вызывается при пометке модели на удаление, до сохранения.
  /// </summary>
  /// <param name="model">Удаляемая модель</param>
  protected virtual void OnModelDeleted(T model)
  {
  }

  /// <summary>
  /// Вызывается после успешной записи изменений репозитория в рамках той же сессии.
  /// Позволяет обновить связанные коллекции (например, денормализованные счётчики) атомарно с основной записью.
  /// </summary>
  /// <param name="sessionHandle">Сессия MongoDB</param>
  /// <param name="token">Токен отмены операции</param>
  protected virtual Task OnCommittedAsync(IClientSessionHandle sessionHandle, CancellationToken token)
  {
    return Task.CompletedTask;
  }

  #endregion

  #region Конструкторы

  /// <summary>
  /// Базовый класс репозитория для работы с MongoDB
  /// </summary>
  /// <param name="config">Конфигурация трекера</param>
  /// <param name="collection">Коллекция MongoDB</param>
  protected RepositoryBase(ModelBuilder config, IMongoCollection<T> collection) : base(config)
  {
    Collection = collection;
  }

  #endregion
}
