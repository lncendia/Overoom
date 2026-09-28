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
    var entity = new MongoDataProtectionKey
    {
      Id = Guid.NewGuid(),

      FriendlyName = friendlyName,

      Xml = element.ToString(SaveOptions.DisableFormatting),

      ExpirationDate = (DateTime?)element.Element("expirationDate")
    };

    _collection.InsertOne(entity);
  }
}