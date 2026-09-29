using Amazon.Runtime;
using Common.Infrastructure.FileStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.DI.HostedServices;

/// <summary>
/// Создаёт бакет файлового хранилища при старте приложения, если его ещё нет.
/// </summary>
/// <remarks>
/// Ошибка не прерывает запуск: в окружениях, где у сервиса нет прав на создание бакетов,
/// бакет создаётся заранее, а приложение продолжает работать с существующим.
/// </remarks>
/// <param name="storage">Клиент S3-совместимого хранилища</param>
/// <param name="logger">Логгер</param>
public class FileStorageBucketInitializer(AwsS3ApiClient storage, ILogger<FileStorageBucketInitializer> logger)
  : IHostedService
{
  /// <inheritdoc/>
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    try
    {
      if (await storage.EnsureBucketExistsAsync(cancellationToken))
        logger.LogInformation("File storage bucket has been created.");
    }
    catch (AmazonServiceException ex)
    {
      logger.LogWarning(ex, "Failed to ensure that the file storage bucket exists.");
    }
  }

  /// <inheritdoc/>
  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
