using Uploader.Infrastructure.Films;
using Common.Infrastructure.FileStorage;
using Uploader.Application.Abstractions.Services;

namespace Uploader.Start.Extensions;

/// <summary>
/// Статический класс расширений для регистрации инфраструктурных сервисов
/// </summary>
public static class HlsServices
{
    /// <summary>
    /// Регистрирует все инфраструктурные сервисы приложения в DI-контейнере
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    public static void AddHlsServices(this IServiceCollection services)
    {
        // Добавление сервиса хранения фильмов.
        services.AddScoped<IHlsStorage, HlsS3Storage>();
        
        // Регистрация именного HttpClient с именем "FileStoreHttpClient".
        services.AddHttpClient(AwsS3ApiClient.HttpClientName);
        
        // Добавление сервиса транскодирования фильмов.
        services.AddSingleton<IHlsTranscodingService, FfmpegHlsTranscodingService>();
    }
}