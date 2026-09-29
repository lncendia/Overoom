using Common.DI.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.DI.Extensions;

/// <summary>
/// Класс устанавливающий CORS конфигурацию
/// </summary>
public static class CorsServices
{
  /// <summary>
  /// Имя политики Cors
  /// </summary>
  public const string CorsPolicy = "CorsPolicy";

  /// <summary>
  /// Устанавливает CORS конфигурацию
  /// </summary>
  /// <param name="builder">Построитель приложения</param>
  public static void AddCorsServices(this IHostApplicationBuilder builder)
  {
    string[]? allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

    if (allowedOrigins == null || allowedOrigins.Length == 0)
      throw new ConfigurationException("Cors:AllowedOrigins");

    bool allowAnyOrigin = allowedOrigins.Length == 1 && allowedOrigins.First() == "*";

    builder.Services.AddCors(options =>
    {
      options.AddPolicy(CorsPolicy, policyBuilder =>
      {
        policyBuilder.WithOrigins(allowedOrigins);
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        if (!allowAnyOrigin) policyBuilder.AllowCredentials();
      });
    });
  }
}