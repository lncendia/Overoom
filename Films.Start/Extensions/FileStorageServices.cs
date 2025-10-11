using Common.Infrastructure.FileStorage;

namespace Films.Start.Extensions;

/// <summary>
/// Статический класс расширений для регистрации инфраструктурных сервисов
/// </summary>
public static class FileStorageServices
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