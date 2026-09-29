using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Identix.Infrastructure.Common.DatabaseInitialization.Converters;

/// <summary>
/// Конвертер JSON для сериализации и десериализации словаря, где ключами являются объекты CultureInfo,
/// а значениями - строки. Преобразует CultureInfo в строковые идентификаторы и обратно.
/// </summary>
public class CultureInfoDictionaryConverter : JsonConverter<Dictionary<CultureInfo, string>>
{
  /// <summary>
  /// Десериализует JSON объект в словарь CultureInfo-string.
  /// </summary>
  /// <param name="reader">JSON reader для чтения данных.</param>
  /// <param name="typeToConvert">Тип для конвертации (Dictionary{CultureInfo, string}).</param>
  /// <param name="options">Опции сериализатора.</param>
  /// <returns>Словарь, где ключи - объекты CultureInfo, значения - строки.</returns>
  public override Dictionary<CultureInfo, string> Read(ref Utf8JsonReader reader, Type typeToConvert,
    JsonSerializerOptions options)
  {
    var dict = new Dictionary<CultureInfo, string>();
    using var doc = JsonDocument.ParseValue(ref reader);

    foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
    {
      var culture = CultureInfo.GetCultureInfo(prop.Name);
      string stringValue = prop.Value.GetString()!;
      dict[culture] = stringValue;
    }

    return dict;
  }

  /// <summary>
  /// Сериализует словарь CultureInfo-string в JSON объект.
  /// </summary>
  /// <param name="writer">JSON writer для записи данных.</param>
  /// <param name="value">Словарь для сериализации.</param>
  /// <param name="options">Опции сериализатора.</param>
  public override void Write(Utf8JsonWriter writer, Dictionary<CultureInfo, string> value,
    JsonSerializerOptions options)
  {
    writer.WriteStartObject();

    foreach (KeyValuePair<CultureInfo, string> kvp in value)
    {
      writer.WriteString(kvp.Key.Name, kvp.Value);
    }

    writer.WriteEndObject();
  }
}