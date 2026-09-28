using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Infrastructure.JsonConverters;

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
      JsonSerializerOptions safeOptions = CreateOptionsWithoutSelf(options);
      return (T?)JsonSerializer.Deserialize(ref reader, typeToConvert, safeOptions);
    }

    if (reader.TokenType != JsonTokenType.StartObject)
      throw new JsonException("Expected StartObject");
    reader.Read();

    if (reader.TokenType != JsonTokenType.PropertyName)
      throw new JsonException("Expected PropertyName for type discriminator");

    string typeName = reader.GetString()!;
    Type? clrType = FindTypeByName(typeName);

    if (clrType == null || !CanConvert(clrType))
      throw new JsonException($"Unknown type '{typeName}' for base type {typeof(T).Name}");

    reader.Read();
    object? value = JsonSerializer.Deserialize(ref reader, clrType, options);
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
    if (value == null)
      throw new ArgumentNullException(nameof(value));

    Type actualType = value.GetType();
    writer.WriteStartObject();
    string typeName = actualType.Name;
    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(typeName) ?? typeName);
    writer.WriteStartObject();

    foreach (PropertyInfo prop in actualType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
      object? propValue = prop.GetValue(value);

      if (propValue == null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
        continue;

      string propName = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
      writer.WritePropertyName(propName);
      JsonSerializer.Serialize(writer, propValue, propValue?.GetType() ?? typeof(object), options);
    }

    writer.WriteEndObject();
    writer.WriteEndObject();
  }

  /// <summary>
  /// Находит тип по имени, который является наследником T или T самим, во всех загруженных сборках.
  /// </summary>
  /// <typeparam name="T">Базовый тип или интерфейс</typeparam>
  /// <param name="typeName">Имя типа для поиска (без учёта регистра)</param>
  /// <returns>Найденный тип или null</returns>
  private static Type? FindTypeByName(string typeName)
  {
    Type baseType = typeof(T);

    return AppDomain.CurrentDomain
      .GetAssemblies()
      .SelectMany(assembly =>
      {
        try
        {
          return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
          return e.Types.Where(t => t != null);
        }
      })
      .FirstOrDefault(t =>
        t != null &&
        baseType.IsAssignableFrom(t) &&
        string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));
  }


  /// <summary>
  /// Создает копию опций сериализации без текущего конвертера
  /// </summary>
  /// <param name="sourceOptions">Исходные опции сериализации</param>
  /// <returns>Новые опции сериализации</returns>
  private JsonSerializerOptions CreateOptionsWithoutSelf(JsonSerializerOptions sourceOptions)
  {
    var clone = new JsonSerializerOptions(sourceOptions);

    JsonConverter? thisConverter = sourceOptions.Converters.FirstOrDefault(c => c == this);
    if (thisConverter != null) clone.Converters.Remove(thisConverter);

    return clone;
  }
}