namespace Rooms.Application.Abstractions.Services;

/// <summary>
/// Счётчики действий зрителей в комнатах, используемые для выдачи тегов.
/// </summary>
/// <remarks>
/// Счётчики живут отдельно от агрегата комнаты: частые события (сообщения, паузы, перемотки) не должны
/// переписывать документ комнаты и конфликтовать между собой по версии.
/// </remarks>
public interface IViewerStatistics
{
  /// <summary>
  /// Атомарно увеличивает счётчик зрителя на 1.
  /// </summary>
  /// <param name="roomId">Идентификатор комнаты</param>
  /// <param name="viewerId">Идентификатор зрителя</param>
  /// <param name="parameter">Название счётчика</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Значение счётчика после увеличения</returns>
  Task<int> IncrementAsync(Guid roomId, Guid viewerId, string parameter, CancellationToken cancellationToken = default);

  /// <summary>
  /// Удаляет все счётчики зрителя в комнате.
  /// </summary>
  /// <param name="roomId">Идентификатор комнаты</param>
  /// <param name="viewerId">Идентификатор зрителя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  Task RemoveViewerAsync(Guid roomId, Guid viewerId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Удаляет счётчики всех зрителей комнаты.
  /// </summary>
  /// <param name="roomId">Идентификатор комнаты</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  Task RemoveRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
}
