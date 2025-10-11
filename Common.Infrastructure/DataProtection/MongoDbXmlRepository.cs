using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Driver;

namespace Common.Infrastructure.DataProtection;

/// <summary>
/// Репозиторий для хранения XML-данных в MongoDB
/// </summary>
/// <param name="client">Клиент MongoDB</param>
/// <param name="databaseName">Имя базы данных</param>
public class MongoDbXmlRepository(IMongoClient client, string databaseName) : IXmlRepository
{
    /// <summary>
    /// Коллекция MongoDB для хранения ключей защиты данных
    /// </summary>
    private readonly IMongoCollection<MongoDataProtectionKey> _collection =
        client.GetDatabase(databaseName).GetCollection<MongoDataProtectionKey>("DataProtectionKeys");

    /// <summary>
    /// Получить все XML-элементы из хранилища
    /// </summary>
    /// <returns>Коллекция XML-элементов только для чтения</returns>
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        // Находим все документы в коллекции, преобразуем в список,
        // парсим XML из каждого документа и возвращаем как read-only коллекцию
        return _collection.Find(_ => true)
            .ToList()
            .Select(x => XElement.Parse(x.Xml))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Сохранить XML-элемент в хранилище
    /// </summary>
    /// <param name="element">XML-элемент для сохранения</param>
    /// <param name="friendlyName">Человеко-читаемое имя элемента</param>
    public void StoreElement(XElement element, string friendlyName)
    {
        // Создаем новую сущность для хранения в MongoDB
        var entity = new MongoDataProtectionKey
        {
            // Уникальный идентификатор
            Id = Guid.NewGuid(),

            // Человеко-читаемое имя
            FriendlyName = friendlyName,

            // XML в виде строки (без форматирования)
            Xml = element.ToString(SaveOptions.DisableFormatting),

            // Дата истечения срока действия (извлекается из XML)
            ExpirationDate = (DateTime?)element.Element("expirationDate")
        };

        // Вставляем документ в коллекцию MongoDB
        _collection.InsertOne(entity);
    }
}