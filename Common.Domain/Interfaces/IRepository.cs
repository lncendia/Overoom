using Common.Domain.Specifications.Abstractions;

namespace Common.Domain.Interfaces;

/// <summary> 
/// Интерфейс репозитория для работы с сущностями. 
/// </summary> 
/// <typeparam name="T">Тип агрегата.</typeparam> 
/// <typeparam name="TK">Тип ключа агрегата.</typeparam> 
/// <typeparam name="TX">Тип посетителя спецификации для агрегата.</typeparam> 
public interface IRepository<T, in TK, out TX> where T : class
    where TX : ISpecificationVisitor<TX, T>
{
    /// <summary> 
    /// Асинхронно добавляет новый агрегат. 
    /// </summary> 
    /// <param name="aggregate">Агрегат для добавления.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param> 
    Task AddAsync(T aggregate, CancellationToken cancellationToken = default);

    /// <summary> 
    /// Асинхронно добавляет новые агрегаты. 
    /// </summary>
    /// <param name="aggregates">Агрегаты для добавления.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param>
    Task AddRangeAsync(IReadOnlyList<T> aggregates, CancellationToken cancellationToken = default);

    /// <summary> 
    /// Асинхронно обновляет информацию о агрегате. 
    /// </summary>
    /// <param name="aggregate">Агрегат для обновления.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param> 
    Task UpdateAsync(T aggregate, CancellationToken cancellationToken = default);

    /// <summary> 
    /// Асинхронно обновляет информацию об агрегатах. 
    /// </summary>
    /// <param name="aggregates">Агрегаты для обновления.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param>
    Task UpdateRangeAsync(IReadOnlyList<T> aggregates, CancellationToken cancellationToken = default);

    /// <summary> 
    /// Асинхронно удаляет агрегат по ее ключу. 
    /// </summary>
    /// <param name="id">Ключ агрегата для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param> 
    Task DeleteAsync(TK id, CancellationToken cancellationToken = default);

    /// <summary> 
    /// Асинхронно удаляет агрегаты по спецификации. 
    /// </summary>
    /// <param name="specification">Спецификация для поиска агрегатов.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param>
    Task<int> DeleteRangeAsync(ISpecification<T, TX> specification, CancellationToken cancellationToken = default);

    /// <summary> 
    /// Асинхронно получает агрегат по ее ключу. 
    /// </summary> 
    /// <param name="id">Ключ агрегата для получения.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param> 
    /// <returns>агрегат с указанным ключом.</returns> 
    Task<T?> GetAsync(TK id, CancellationToken cancellationToken = default);

    /// <summary> 
    /// Асинхронно выполняет поиск агрегатов, удовлетворяющих указанной спецификации, с возможностью сортировки, пропуска и взятия определенного количества. 
    /// </summary>
    /// <param name="specification">Спецификация для поиска агрегатов.</param>
    /// <param name="skip">Количество пропускаемых агрегатов.</param>
    /// <param name="take">Количество выбираемых агрегатов.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param> 
    /// <returns>Список агрегатов, удовлетворяющих условиям поиска.</returns> 
    Task<IReadOnlyList<T>> FindAsync(
        ISpecification<T, TX>? specification,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default
    );

    /// <summary> 
    /// Асинхронно возвращает первый агрегат, удовлетворяющих указанной спецификации или значение по умолчанию. 
    /// </summary>
    /// <param name="specification">Спецификация для поиска агрегатов.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param> 
    /// <returns>Список агрегатов, удовлетворяющих условиям поиска.</returns> 
    Task<T?> FirstOrDefaultAsync(
        ISpecification<T, TX>? specification,
        CancellationToken cancellationToken = default
    );
    
    /// <summary> 
    /// Возвращает количество агрегатов, удовлетворяющих указанной спецификации. 
    /// </summary>
    /// <param name="specification">Спецификация для поиска агрегатов.</param>
    /// <param name="cancellationToken">Токен для отмены операции</param> 
    /// <returns>Количество агрегатов, удовлетворяющих условиям поиска.</returns> 
    Task<int> CountAsync(ISpecification<T, TX>? specification, CancellationToken cancellationToken = default);
}