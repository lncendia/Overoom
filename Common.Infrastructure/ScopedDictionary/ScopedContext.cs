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
    private static readonly AsyncLocal<Stack<ScopedDictionary>?> Scopes = new();

    /// <summary>
    /// Получает текущую активную область видимости
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается если нет активных областей видимости
    /// </exception>
    public IScopedDictionary Current =>
        Scopes.Value is { Count: > 0 }
            ? Scopes.Value.Peek() // Возвращаем верхний элемент стека (текущую область)
            : throw new InvalidOperationException("No active scopes available.");

    /// <summary>
    /// Определяет, находится ли выполнение в области видимости.
    /// </summary>
    public bool InScope => Scopes.Value is { Count: > 0 };

    /// <summary>
    /// Создает новую область видимости и возвращает disposable для ее освобождения
    /// </summary>
    /// <returns>Disposable объект для управления временем жизни области видимости</returns>
    public IDisposable CreateScope()
    {
        // Инициализируем стек если он еще не создан
        Scopes.Value ??= new Stack<ScopedDictionary>();

        // Создаем новую область видимости
        var scope = new ScopedDictionary();
        
        // Добавляем новую область в стек
        Scopes.Value.Push(scope);

        // Возвращаем disposer для корректного удаления области при выходе
        return new ScopeDisposer(Scopes.Value);
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
            // Защита от многократного вызова Dispose
            if (_disposed || stack.Count <= 0) return;
            
            // Удаляем текущую область из стека
            stack.Pop();
            _disposed = true;
        }
    }
}