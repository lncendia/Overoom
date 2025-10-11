using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;
using Films.Domain.Films;
using Films.Domain.Films.Specifications.Visitor;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Visitors;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения фильмов.
/// </summary>
public class FilmRepository(MongoDbContext context) : RepositoryBase<FilmModel, Film>(c => c.Id, context.Films), IFilmRepository
{
    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно добавляет новый агрегат. 
    /// </summary> 
    public Task AddAsync(Film aggregate, CancellationToken cancellationToken = default)
    {
        // Преобразуем агрегат в модель данных, которая будет использоваться для хранения данных в базе данных.
        var model = new FilmModel { Id = aggregate.Id };
        
        // Заполняем модель из снапшота
        model.UpdateFromSnapshot(aggregate.GetSnapshot());

        // Начинаем отслеживать добавляемую модель
        Add(model, aggregate);

        // Возвращаем завершенную задачу.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно добавляет новые агрегаты. 
    /// </summary>
    public Task AddRangeAsync(IReadOnlyList<Film> aggregates, CancellationToken cancellationToken = default)
    {
        // Выполняем код для каждого агрегата
        foreach (var aggregate in aggregates)
        {
            // Преобразуем агрегат в модель данных, которая будет использоваться для хранения данных в базе данных.
            var model = new FilmModel { Id = aggregate.Id };

            // Заполняем модель из снапшота
            model.UpdateFromSnapshot(aggregate.GetSnapshot());

            // Начинаем отслеживать добавляемую модель
            Add(model, aggregate);
        }

        // Возвращаем завершенную задачу.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно обновляет информацию об агрегате. 
    /// </summary> 
    public Task UpdateAsync(Film aggregate, CancellationToken cancellationToken = default)
    {
        // Получаем сущность, уже отслеживается в контексте.
        var model = Get(aggregate.Id);

        // Преобразуем изменения из агрегата в модель данных, используя снапшоты.
        model.UpdateFromSnapshot(aggregate.GetSnapshot());
            
        // Добавляем события агрегата
        Update(aggregate);

        // Возвращаем завершенную задачу.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно обновляет информацию об агрегатах. 
    /// </summary>
    public Task UpdateRangeAsync(IReadOnlyList<Film> aggregates, CancellationToken cancellationToken = default)
    {
        // Выполняем код для каждого агрегата
        foreach (var aggregate in aggregates)
        {
            // Получаем сущность, уже отслеживается в контексте.
            var model = Get(aggregate.Id);

            // Преобразуем изменения из агрегата в модель данных, используя снапшоты.
            model.UpdateFromSnapshot(aggregate.GetSnapshot());
            
            // Добавляем события агрегата
            Update(aggregate);
        }

        // Возвращаем завершенную задачу.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно удаляет агрегат по ее ключу. 
    /// </summary> 
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Получаем сущность как удаляемую
        Delete(id);

        // Возвращаем завершенную задачу.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно удаляет агрегаты по спецификации. 
    /// </summary>
    public async Task<int> DeleteRangeAsync(ISpecification<Film, IFilmSpecificationVisitor> specification,
        CancellationToken cancellationToken = default)
    {
        // Получаем базовый запрос к коллекции Film из контекста.
        var query = Collection.AsQueryable();

        // Создаем экземпляр посетителя спецификаций (visitor), который будет использоваться для преобразования спецификации в выражение LINQ.
        var visitor = new FilmVisitor();

        // Применяем спецификацию через паттерн "Посетитель".
        specification.Accept(visitor);

        // Если посетитель сгенерировал выражение фильтрации (visitor.Expr != null),
        // добавляем его к базовому запросу с помощью метода Where().
        if (visitor.Expr != null) query = query.Where(visitor.Expr);

        // Выполняем запрос асинхронно и получаем список сущностей, удовлетворяющих спецификации.
        var entities = await query.ToListAsync(cancellationToken: cancellationToken);

        // Получаем каждую сущность как удаляемую
        foreach (var trackedEntity in entities.Select(Track))
        {
            // Получаем сущность как удаляемую
            Delete(trackedEntity.Id);
        }

        // Возвращаем количество удаленных сущностей.
        return entities.Count;
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно выполняет поиск агрегатов, удовлетворяющих указанной спецификации, с возможностью сортировки, пропуска и взятия определенного количества. 
    /// </summary> 
    public async Task<Film?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Получение сущности из контекста вместе с зависимым объектом
        var model = await Collection.AsQueryable()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        // Проверяем, была ли найдена сущность. Если нет, возвращаем null.
        if (model == null) return null;

        // Обновляем состояние модели, вызывая метод Track(). Это гарантирует, что модель отслеживается контекстом.
        model = Track(model);

        // Возвращение объекта, отображенного на агрегат, если он существует
        return Film.FromSnapshot(model.GetSnapshot());
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно выполняет поиск агрегатов, удовлетворяющих указанной спецификации, с возможностью сортировки, пропуска и взятия определенного количества. 
    /// </summary> 
    public async Task<IReadOnlyList<Film>> FindAsync(
        ISpecification<Film, IFilmSpecificationVisitor>? specification,
        int? skip = null, int? take = null,
        CancellationToken cancellationToken = default)
    {
        // Получение запроса на выборку из базы данных
        var query = Collection.AsQueryable();

        // Если задана спецификация
        if (specification != null)
        {
            // Создаем посетитель спецификаций инстанса
            var visitor = new FilmVisitor();

            // Посещаем спецификацию
            specification.Accept(visitor);

            // Добавляем к запросу полученную выборку
            if (visitor.Expr != null) query = query.Where(visitor.Expr);
        }

        // Сортируем по Id
        query = query.OrderBy(i => i.Id);

        // Если установлено значение пропускаемых записей - устанавливаем в запрос
        if (skip.HasValue) query = query.Skip(skip.Value);

        // Если установлено значение получаемых записей - устанавливаем в запрос
        if (take.HasValue) query = query.Take(take.Value);

        // Выполняем запрос асинхронно и получаем список моделей из базы данных.
        var models = await query.ToListAsync(cancellationToken: cancellationToken);

        // Преобразуем каждую модель в агрегат с помощью цепочки методов:
        return models
            .Select(Track) // Для каждой модели вызываем метод Track(), чтобы убедиться, что она отслеживается контекстом.
            .Select(m => Film.FromSnapshot(m.GetSnapshot())) // Преобразуем каждую модель в агрегат с помощью маппера ReportMapper.Map().
            .ToArray(); // Преобразуем результат в массив и возвращаем его.
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Асинхронно возвращает первый агрегат, удовлетворяющих указанной спецификации или значение по умолчанию. 
    /// </summary>
    public async Task<Film?> FirstOrDefaultAsync(
        ISpecification<Film, IFilmSpecificationVisitor>? specification,
        CancellationToken cancellationToken = default
    )
    {
        // Получаем первую страницу результатов (1 элемент) согласно спецификации
        var results = await FindAsync(
            specification: specification,
            skip: 0, // Пропускаем 0 элементов
            take: 1, // Берем только первый элемент
            cancellationToken: cancellationToken);

        // Возвращаем первый элемент из результатов или null, если коллекция пуста
        return results.FirstOrDefault();
    }

    /// <inheritdoc/>
    /// <summary> 
    /// Возвращает количество агрегатов, удовлетворяющих указанной спецификации. 
    /// </summary> 
    public async Task<int> CountAsync(ISpecification<Film, IFilmSpecificationVisitor>? specification,
        CancellationToken cancellationToken = default)
    {
        // Получение запроса на выборку из базы данных
        var query = Collection.AsQueryable();

        // Если спецификация не задана, возврат общего числа записей в запросе
        if (specification == null) return await query.CountAsync(cancellationToken: cancellationToken);

        // Создаем посетитель спецификаций отчетов
        var visitor = new FilmVisitor();

        // Посещаем спецификацию
        specification.Accept(visitor);

        // Добавляем к запросу полученную выборку
        if (visitor.Expr != null) query = query.Where(visitor.Expr);

        // Выполняем запрос и возвращаем результат
        return await query.CountAsync(cancellationToken: cancellationToken);
    }
}