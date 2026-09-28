using Common.Application.ScopedDictionary;

namespace Common.Infrastructure.ScopedDictionary;

/// <summary>
/// Реализация контекста скопов с поддержкой async/await.
/// Предоставляет механизм для создания изолированных областей видимости данных,
/// которые сохраняются в рамках асинхронного потока выполнения.
/// </summary>
public class ScopedContext : IScopedContext
{
  // AsyncLocal обеспечивает хранение данных в рамках асинхронного контекста
  // Stack<ScopedDictionary> хранит вложенные области видимости
  private static readonly AsyncLocal<Stack<ScopedDictionary>?> _scopes = new();

  /// <summary>
  /// Получает текущую активную область видимости
  /// </summary>
  /// <exception cref="InvalidOperationException">
  /// Выбрасывается если нет активных областей видимости
  /// </exception>
  public IScopedDictionary Current =>
    _scopes.Value is { Count: > 0 }
      ? _scopes.Value.Peek() // Возвращаем верхний элемент стека (текущую область)
      : throw new InvalidOperationException("No active scopes available.");

  /// <summary>
  /// Определяет, находится ли выполнение в области видимости.
  /// </summary>
  public bool InScope => _scopes.Value is { Count: > 0 };

  /// <summary>
  /// Создает новую область видимости и возвращает disposable для ее освобождения
  /// </summary>
  /// <returns>Disposable объект для управления временем жизни области видимости</returns>
  public IDisposable CreateScope()
  {
    _scopes.Value ??= new Stack<ScopedDictionary>();
    var scope = new ScopedDictionary();
    _scopes.Value.Push(scope);

    return new ScopeDisposer(_scopes.Value);
  }

  /// <summary>
  /// Внутренний класс для управления временем жизни области видимости
  /// Автоматически удаляет область из стека при вызове Dispose
  /// </summary>
  private class ScopeDisposer(Stack<ScopedDictionary> stack) : IDisposable
  {
    private bool _disposed;

    /// <summary>
    /// Освобождает текущую область видимости, удаляя ее из стека
    /// </summary>
    public void Dispose()
    {
      if (_disposed || stack.Count <= 0) return;

      stack.Pop();
      _disposed = true;
    }
  }
}