namespace Common.Application.ScopedDictionary;

/// <summary>
/// Интерфейс контекста скопа.
/// </summary>
public interface IScopedContext
{
    /// <summary>
    /// Текущий активный скоп (или null, если скопа нет).
    /// </summary>
    IScopedDictionary Current { get; }
    
    /// <summary>
    /// Определяет, находится ли выполнение в области видимости.
    /// </summary>
    bool InScope { get; }

    /// <summary>
    /// Создаёт новый скоп и делает его текущим.
    /// </summary>
    IDisposable CreateScope();
}