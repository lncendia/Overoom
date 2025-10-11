namespace Rooms.Application.Abstractions.Services;

/// <summary>
/// Интерфейс для сервиса очистки сообщений в комнатах чата
/// </summary>
public interface IMessagesCleaner
{
    /// <summary>
    /// Удаляет все сообщения, принадлежащие указанной комнате
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты, сообщения которой необходимо удалить</param>
    /// <param name="cancellationToken">Токен отмены для прерывания операции</param>
    /// <returns>Задача, представляющая асинхронную операцию очистки</returns>
    Task CleanAsync(Guid roomId, CancellationToken cancellationToken = default);
}