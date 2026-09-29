namespace Common.Domain.Aggregates;

/// <summary>
/// Контракт объекта, состояние которого сохраняется и восстанавливается через снапшот.
/// </summary>
/// <remarks>
/// Реализуйте члены явно, чтобы они не попадали в публичный API доменной модели
/// и вызывались только инфраструктурой через ограничение generic-параметра.
/// </remarks>
/// <typeparam name="TSelf">Тип восстанавливаемого объекта</typeparam>
/// <typeparam name="TSnapshot">Тип снапшота</typeparam>
public interface ISnapshotable<TSelf, TSnapshot> where TSelf : ISnapshotable<TSelf, TSnapshot>
{
  /// <summary>
  /// Восстанавливает объект из снапшота без выполнения бизнес-логики и генерации доменных событий.
  /// </summary>
  /// <param name="snapshot">Снапшот состояния</param>
  /// <returns>Восстановленный объект</returns>
  static abstract TSelf Restore(TSnapshot snapshot);

  /// <summary>
  /// Создаёт снапшот текущего состояния объекта.
  /// </summary>
  /// <returns>Снапшот состояния</returns>
  TSnapshot ToSnapshot();
}
