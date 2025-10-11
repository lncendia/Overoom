using MongoDB.Driver;
using Rooms.Application.Abstractions.Services;
using Rooms.Infrastructure.Storage.Context;
using Rooms.Infrastructure.Storage.Models.Messages;

namespace Rooms.Infrastructure.Storage.Services;

/// <summary>
/// Реализация сервиса очистки сообщений в комнатах чата с использованием MongoDB
/// </summary>
public class MessagesCleaner(MongoDbContext context) : IMessagesCleaner
{
    /// <summary>
    /// Коллекция сообщений в MongoDB для выполнения операций удаления
    /// </summary>
    private readonly IMongoCollection<MessageModel> _messages = context.Messages;

    /// <summary>
    /// Удаляет все сообщения, принадлежащие указанной комнате
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты, сообщения которой необходимо удалить</param>
    /// <param name="cancellationToken">Токен отмены для прерывания операции</param>
    /// <returns>Задача, представляющая асинхронную операцию очистки</returns>
    public async Task CleanAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        // Создаем фильтр для поиска всех сообщений в указанной комнате
        var filter = Builders<MessageModel>.Filter.Eq(x => x.RoomId, roomId);
        
        // Выполняем массовое удаление всех сообщений, соответствующих фильтру
        await _messages.DeleteManyAsync(filter, cancellationToken);
    }
}