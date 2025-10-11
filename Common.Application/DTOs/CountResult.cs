namespace Common.Application.DTOs;

/// <summary>
/// Результат подсчета количества элементов и их получения.
/// </summary>
/// <typeparam name="T">Тип элементов.</typeparam>
public class CountResult<T>
{
    /// <summary>
    /// Коллекция элементов.
    /// </summary>
    public required IReadOnlyList<T> List { get; init; }

    /// <summary>
    /// Общее количество элементов.
    /// </summary>
    public required int TotalCount { get; init; }
    
    /// <summary>
    /// Фабричный метод для пустого CountResult
    /// </summary>
    /// <returns></returns>
    public static CountResult<T> NoValues()
    {
        // Возвращаем пустой CountResult
        return new CountResult<T>
        {
            List = [],
            TotalCount = 0
        };
    }
}