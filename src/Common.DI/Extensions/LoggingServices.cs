using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Common.DI.Extensions;

/// <summary>
/// Статический класс для регистрации Serilog в контейнере DI
/// </summary>
public static class LoggingServices
{
  /// <summary>
  /// Метод регистрирует Serilog в контейнере DI
  /// </summary>
  /// <param name="builder">Построитель веб-приложений и сервисов.</param>
  public static void AddLoggingServices(this WebApplicationBuilder builder)
  {
    Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(builder.Configuration)
      .CreateLogger();

    builder.Host.UseSerilog();
  }
}
