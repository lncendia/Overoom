using Amazon.Runtime;

namespace Common.Infrastructure.FileStorage;

/// <summary>
/// Фабрика для создания экземпляров HttpClient, используемых для взаимодействия с AWS S3
/// Позволяет использовать заранее сконфигурированный HttpClient для AWS операций
/// </summary>
/// <param name="httpClient">Экземпляр HttpClient, используемый для выполнения HTTP-запросов</param>
public sealed class AwsS3HttpClientFactory(HttpClient httpClient) : HttpClientFactory
{
    /// <summary>
    /// Создает экземпляр HttpClient с заданной конфигурацией клиента
    /// </summary>
    /// <param name="clientConfig">Конфигурация клиента, используемая для настройки HttpClient</param>
    /// <returns>Экземпляр HttpClient, настроенный в соответствии с конфигурацией клиента</returns>
    public override HttpClient CreateHttpClient(IClientConfig clientConfig) => httpClient;
}