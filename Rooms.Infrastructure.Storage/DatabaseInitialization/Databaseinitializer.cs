using Microsoft.Extensions.DependencyInjection;
using Rooms.Infrastructure.Storage.Context;

namespace Rooms.Infrastructure.Storage.DatabaseInitialization;

/// <summary>
/// Класс для инициализации начальных данных в базу данных
/// </summary>
public static class DatabaseInitializer
{
  /// <summary>
  /// Инициализация начальных данных в базу данных
  /// </summary>
  /// <param name="scopeServiceProvider">Определяет механизм для извлечения объекта службы,
  /// т. е. объекта, обеспечивающего настраиваемую поддержку для других объектов.</param>
  public static async Task InitAsync(IServiceProvider scopeServiceProvider)
  {
    MongoDbContext context = scopeServiceProvider.GetRequiredService<MongoDbContext>();
    await context.EnsureCreatedAsync();
  }
}