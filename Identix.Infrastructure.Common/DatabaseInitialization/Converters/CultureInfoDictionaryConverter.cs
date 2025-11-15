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
        
        // Парсинг JSON документа
        using var doc = JsonDocument.ParseValue(ref reader);
        
        // Итерация по всем свойствам JSON объекта
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            // Преобразование имени свойства (строки) в объект CultureInfo
            var culture = CultureInfo.GetCultureInfo(prop.Name);
            
            // Получение строкового значения свойства
            var stringValue = prop.Value.GetString()!;
            
            // Добавление пары ключ-значение в словарь
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
        // Начало записи JSON объекта
        writer.WriteStartObject();
        
        // Итерация по всем элементам словаря
        foreach (var kvp in value)
        {
            // Запись пары ключ-значение, где ключ - имя культуры (например, "ru-RU")
            writer.WriteString(kvp.Key.Name, kvp.Value);
        }
        
        // Завершение записи JSON объекта
        writer.WriteEndObject();
    }
}