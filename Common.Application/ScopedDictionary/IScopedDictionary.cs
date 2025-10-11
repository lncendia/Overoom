namespace Common.Application.ScopedDictionary;

/// <summary>
/// Интерфейс для работы с временным словарем, который хранит данные в рамках одного запроса или контекста.
/// </summary>
public interface IScopedDictionary
{
    /// <summary>
    /// Добавляет значение в словарь по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет сохранено значение.</param>
    /// <param name="value">Значение, которое нужно сохранить.</param>
    void Add(string key, object value);

    /// <summary>
    /// Получает значение из словаря по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно получить значение.</param>
    /// <returns>Значение, связанное с указанным ключом, или null, если ключ не найден.</returns>
    T Get<T>(string key);

    /// <summary>
    /// Индексатор для доступа к значениям в словаре по ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно получить или установить значение.</param>
    /// <returns>Значение, связанное с указанным ключом.</returns>
    object this[string key] { get; set; }

    /// <summary>
    /// Пытается получить значение из словаря по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно получить значение.</param>
    /// <param name="value">Значение, связанное с указанным ключом, или default если ключ не найден.</param>
    /// <returns>True, если ключ найден и значение успешно приведено к типу <typeparamref name="T"/>; иначе false.</returns>
    bool TryGetValue<T>(string key, out T? value);
}