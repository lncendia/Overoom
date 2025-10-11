using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rooms.Infrastructure.Web.JsonConverters;

/// <summary>
/// JSON конвертер для сериализации и десериализации объектов с использованием имени типа в качестве дискриминатора
/// </summary>
/// <typeparam name="T">Базовый тип для сериализации</typeparam>
public class TypeNameJsonConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// Определяет, может ли конвертер преобразовать указанный тип
    /// </summary>
    /// <param name="typeToConvert">Тип для проверки</param>
    /// <returns>True если тип может быть преобразован, иначе False</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(T).IsAssignableFrom(typeToConvert);
    }

    /// <summary>
    /// Читает и десериализует JSON в объект указанного типа
    /// </summary>
    /// <param name="reader">JSON reader</param>
    /// <param name="typeToConvert">Тип для десериализации</param>
    /// <param name="options">Опции сериализации</param>
    /// <returns>Десериализованный объект или null</returns>
    /// <exception cref="JsonException">Выбрасывается при ошибках формата JSON</exception>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeof(T) != typeToConvert)
        {
            var safeOptions = CreateOptionsWithoutSelf(options);
            return (T?)JsonSerializer.Deserialize(ref reader, typeToConvert, safeOptions);
        }

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject");
        reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException("Expected PropertyName for type discriminator");

        var typeName = reader.GetString()!;
        var clrType = FindTypeByName(typeName);

        if (clrType == null || !CanConvert(clrType))
            throw new JsonException($"Unknown type '{typeName}' for base type {typeof(T).Name}");

        reader.Read();
        var value = JsonSerializer.Deserialize(ref reader, clrType, options);
        reader.Read();

        return (T?)value;
    }

    /// <summary>
    /// Сериализует объект в JSON с использованием имени типа в качестве дискриминатора
    /// </summary>
    /// <param name="writer">JSON writer</param>
    /// <param name="value">Объект для сериализации</param>
    /// <param name="options">Опции сериализации</param>
    /// <exception cref="ArgumentNullException">Выбрасывается если значение равно null</exception>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Проверка на null входного значения
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        // Получаем реальный тип объекта (может быть производным от T)
        var actualType = value.GetType();

        // Начинаем запись JSON объекта
        writer.WriteStartObject();

        // Получаем имя типа для использования как имя свойства
        var typeName = actualType.Name;

        // Записываем имя свойства (имя типа) с учетом политики именования
        writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(typeName) ?? typeName);

        // Начинаем запись вложенного объекта для свойств
        writer.WriteStartObject();

        // Перебираем все публичные свойства типа
        foreach (var prop in actualType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Получаем значение свойства
            var propValue = prop.GetValue(value);

            // Пропускаем null-значения если настроено игнорирование null
            if (propValue == null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
                continue;

            // Преобразуем имя свойства согласно политике именования (по умолчанию camelCase)
            var propName = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);

            // Записываем имя свойства
            writer.WritePropertyName(propName);

            // Сериализуем значение свойства с учетом его типа
            JsonSerializer.Serialize(writer, propValue, propValue?.GetType() ?? typeof(object), options);
        }

        // Завершаем запись вложенного объекта
        writer.WriteEndObject();

        // Завершаем запись основного объекта
        writer.WriteEndObject();
    }

    /// <summary>
    /// Находит тип по имени в сборке базового типа
    /// </summary>
    /// <param name="typeName">Имя типа для поиска</param>
    /// <returns>Найденный тип или null</returns>
    private static Type? FindTypeByName(string typeName)
    {
        return typeof(T).Assembly.GetTypes()
            .FirstOrDefault(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Создает копию опций сериализации без текущего конвертера
    /// </summary>
    /// <param name="sourceOptions">Исходные опции сериализации</param>
    /// <returns>Новые опции сериализации</returns>
    private JsonSerializerOptions CreateOptionsWithoutSelf(JsonSerializerOptions sourceOptions)
    {
        var clone = new JsonSerializerOptions(sourceOptions);

        var thisConverter = sourceOptions.Converters.FirstOrDefault(c => c == this);
        if (thisConverter != null) clone.Converters.Remove(thisConverter);

        return clone;
    }
}