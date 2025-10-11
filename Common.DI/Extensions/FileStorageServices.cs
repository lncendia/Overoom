using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Common.Application.FileStorage;
using Common.Infrastructure.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.DI.Extensions;

/// <summary>
/// Статический класс расширений для регистрации инфраструктурных сервисов
/// </summary>
public static class FileStorageServices
{
    /// <summary>
    /// Имя секции конфигурации по умолчанию для настроек S3 клиента
    /// </summary>
    private const string ConfigurationSectionName = "MinIO";

    /// <summary>
    /// Регистрирует все инфраструктурные сервисы приложения в DI-контейнере
    /// </summary>
    /// <param name="builder">Построитель веб-приложения</param>
    public static void AddFileStorage(this IHostApplicationBuilder builder)
    {
        // Получаем настройки S3 клиента из конфигурации
        var section = builder.Configuration.GetSection(ConfigurationSectionName);

        var defaultBucket = section.GetRequiredValue<string>("DefaultBucket");
        var accessKey = section.GetRequiredValue<string>("AccessKey");
        var secretKey = section.GetRequiredValue<string>("SecretKey");
        var baseAddress = section.GetRequiredValue<string>("BaseAddress");
        var region = section.GetRequiredValue<string>("Region");
        var maxErrorRetry = section.GetValue<int?>("MaxErrorRetry");

        // Регистрируем фабрику HTTP-клиентов для работы с S3
        builder.Services.AddHttpClient<AwsS3HttpClientFactory>();

        // Регистрируем реализацию файлового хранилища на основе S3
        builder.Services.AddSingleton<IFileStorage, AwsS3ApiClient>(sp => new AwsS3ApiClient(
            sp.GetRequiredService<IAmazonS3>(),
            sp.GetRequiredService<IHttpClientFactory>(),
            sp.GetRequiredService<AwsS3HttpClientFactory>(),
            defaultBucket));

        // Настраиваем и регистрируем клиент Amazon S3XL: 
        builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
        {
            Credentials = new BasicAWSCredentials(accessKey, secretKey),
            DefaultClientConfig =
            {
                ServiceURL = baseAddress,
                MaxErrorRetry = maxErrorRetry
            },
            Region = RegionEndpoint.GetBySystemName(region)
        });
        
        // Регистрация именного HttpClient с именем AwsS3ApiClient.HttpClientName.
        builder.Services.AddHttpClient(AwsS3ApiClient.HttpClientName);
    }
}