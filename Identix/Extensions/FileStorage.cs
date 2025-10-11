using Common.Infrastructure.FileStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Identix.Extensions;

/// <summary>
/// Статический класс расширений для регистрации инфраструктурных сервисов
/// </summary>
public static class FileStorage
{
    /// <summary>
    /// Регистрирует все инфраструктурные сервисы приложения в DI-контейнере
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    public static void AddFileStorageHttpClient(this IServiceCollection services)
    {
        // Регистрация именного HttpClient с именем "FileStoreHttpClient".
        services.AddHttpClient(AwsS3ApiClient.HttpClientName, client =>
        {
            // Устанавливаем заголовок Accept для указания, что клиент принимает только изображения.
            client.DefaultRequestHeaders.Add("Accept", "image/*");
        });
    }
}