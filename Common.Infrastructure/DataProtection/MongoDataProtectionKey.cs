using MongoDB.Bson.Serialization.Attributes;

namespace Common.Infrastructure.DataProtection;

/// <summary>
/// Модель документа для хранения ключей защиты данных в MongoDB
/// </summary>
public class MongoDataProtectionKey
{
    /// <summary>
    /// Уникальный идентификатор ключа в коллекции
    /// </summary>
    /// <remarks>
    /// Помечен атрибутом [BsonId] для указания первичного ключа в MongoDB
    /// </remarks>
    [BsonId]
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Человеко-читаемое имя ключа
    /// </summary>
    /// <remarks>
    /// Используется для идентификации ключа в системе
    /// </remarks>
    public required string FriendlyName { get; init; }
    
    /// <summary>
    /// XML-представление ключа защиты данных
    /// </summary>
    /// <remarks>
    /// Содержит сериализованные данные ключа в формате XML
    /// </remarks>
    public required string Xml { get; init; }
    
    /// <summary>
    /// Дата истечения срока действия ключа
    /// </summary>
    /// <remarks>
    /// Может быть null, если срок действия не установлен
    /// </remarks>
    public required DateTime? ExpirationDate { get; init; }
}