using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Identix.Application.Abstractions.Extensions;

namespace Identix.Extensions;

/// <summary>
/// Статический класс, представляющий методы для добавления сервисов локализации.
/// </summary>
public static class Localization
{
  /// <summary>
  /// Добавляет сервисы локализации в коллекцию сервисов.
  /// </summary>
  /// <param name="services">Коллекция сервисов.</param>
  public static void AddLocalizationServices(this IServiceCollection services)
  {
    services.AddLocalization(options => options.ResourcesPath = "Resources");

    services.Configure<RequestLocalizationOptions>(options =>
    {
      CultureInfo[] supportedCultures =
      [
        new CultureInfo(LocalizationExtensions.En),
        new CultureInfo(LocalizationExtensions.Ru)
      ];

      options.DefaultRequestCulture = new RequestCulture("en", "en");
      options.SupportedCultures = supportedCultures;
      options.SupportedUICultures = supportedCultures;
    });
  }
}