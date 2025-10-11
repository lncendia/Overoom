using Common.Application.ScopedDictionary;

namespace Common.Infrastructure.ScopedDictionary;

/// <summary>
/// Реализация интерфейса IScopedDictionary для хранения данных в рамках одного запроса или контекста.
/// </summary>
public class ScopedDictionary : IScopedDictionary
{
    /// <summary>
    /// Приватный словарь для хранения данных
    /// </summary>
    private readonly Dictionary<string, object> _dictionary = new();

    /// <summary>
    /// Добавляет значение в словарь по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет сохранено значение.</param>
    /// <param name="value">Значение, которое нужно сохранить.</param>
    public void Add(string key, object value)
    {
        // Проверка на null или пустой ключ
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException(@"Key cannot be null or empty.", nameof(key));
        }

        // Добавление значения в словарь
        _dictionary[key] = value;
    }

    /// <summary>
    /// Получает значение из словаря по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно получить значение.</param>
    /// <returns>Значение, связанное с указанным ключом, или null, если ключ не найден.</returns>
    public T Get<T>(string key)
    {
        // Проверка на null или пустой ключ
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException(@"Key cannot be null or empty.", nameof(key));
        }

        // Возвращаем значение по ключу
        return (T)_dictionary[key];
    }

    /// <summary>
    /// Индексатор для доступа к значениям в словаре по ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно получить или установить значение.</param>
    /// <returns>Значение, связанное с указанным ключом.</returns>
    public object this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }
    
    /// <summary>
    /// Пытается получить значение из словаря по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно получить значение.</param>
    /// <param name="value">Значение, связанное с указанным ключом, или default если ключ не найден.</param>
    /// <returns>True, если ключ найден и значение успешно приведено к типу <typeparamref name="T"/>; иначе false.</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException(@"Key cannot be null or empty.", nameof(key));
        }

        if (_dictionary.TryGetValue(key, out var obj) && obj is T casted)
        {
            value = casted;
            return true;
        }

        value = default;
        return false;
    }
}