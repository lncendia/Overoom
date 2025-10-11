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
        // Добавляет службы, необходимые для локализации приложения.
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Регистрирует действие
        services.Configure<RequestLocalizationOptions>(options =>
        {
            // поддерживаемые культуры
            var supportedCultures = new[]
            {
                // английский
                new CultureInfo(LocalizationExtensions.En),
                // русский
                new CultureInfo(LocalizationExtensions.Ru),
            };

            // Задает культуру по умолчанию
            options.DefaultRequestCulture = new RequestCulture("en", "en");
            
            // Культуры, поддерживаемые приложением
            options.SupportedCultures = supportedCultures;
            
            // Культуры пользовательского интерфейса
            options.SupportedUICultures = supportedCultures;
        });
    }
}