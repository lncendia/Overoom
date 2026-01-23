namespace Common.Infrastructure.Repositories.Models;

/// <summary>
/// Контракт модели хранения, поддерживающей синхронизацию с доменным снапшотом.
/// </summary>
/// <typeparam name="TS">Тип снапшота доменной сущности</typeparam>
public interface IModel<TS>
{
  /// <summary>
  /// Уникальный идентификатор сущности.
  /// </summary>
  public Guid Id { get; init; }

  /// <summary>
  /// Обновляет состояние модели на основе переданного снапшота.
  /// </summary>
  /// <param name="snapshot">Снапшот доменной сущности</param>
  public void UpdateFromSnapshot(TS snapshot);

  /// <summary>
  /// Формирует снапшот текущего состояния модели.
  /// </summary>
  /// <returns>Снапшот доменной сущности</returns>
  public TS GetSnapshot();
}
