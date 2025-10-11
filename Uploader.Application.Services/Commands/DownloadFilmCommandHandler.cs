using Uploader.Application.Abstractions.Commands;
using Uploader.Application.Abstractions.Services;
using Common.IntegrationEvents.Uploader;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Uploader.Application.Services.Commands;

/// <summary>
/// Обработчик команды загрузки и обработки фильма
/// </summary>
/// <remarks>
/// Выполняет полный цикл обработки фильма:
/// 1. Загрузка по magnet-ссылке
/// 2. Транскодирование в различные разрешения
/// 3. Загрузка в файловое хранилище
/// 4. Публикация события о завершении обработки
/// </remarks>
/// <param name="download">Сервис загрузки фильмов</param>
/// <param name="transcoder">Сервис транскодирования видео</param>
/// <param name="filmStorage">Хранилище файлов фильмов</param>
/// <param name="publishEndpoint">Конечная точка для публикации событий</param>
/// <param name="logger">Логгер</param>
public class DownloadFilmCommandHandler(
    IFilmDownloadService download,
    IHlsTranscodingService transcoder,
    IHlsStorage filmStorage,
    IPublishEndpoint publishEndpoint,
    ILogger<DownloadFilmCommandHandler> logger)
    : IRequestHandler<DownloadFilmCommand>
{
    /// <summary>
    /// Обрабатывает команду загрузки фильма
    /// </summary>
    /// <param name="request">Команда загрузки, содержащая параметры фильма</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    /// <exception cref="OperationCanceledException">Выбрасывается при отмене операции через cancellationToken</exception>
    public async Task Handle(DownloadFilmCommand request, CancellationToken cancellationToken)
    {
        // Логирование начала загрузки
        logger.LogInformation("Downloading a movie by URI: {Uri}", request.MagnetUri);

        // Загрузка оригинального файла по magnet-ссылке
        var originalFilePath = await download.DownloadAsync(request.MagnetUri, request.Filename, cancellationToken);

        // Логирование успешной загрузки
        logger.LogInformation("The movie is downloaded: {Path}", originalFilePath);

        // Формирование пути для конвертированного файла
        var outputPath = Path.Join(Path.GetDirectoryName(originalFilePath), $"hls_{request.FilmRecord.Season ?? 0}_{request.FilmRecord.Episode ?? 0}");

        // Выполнение транскодирования видеофайла в указанное разрешение
        await transcoder.TranscodeAsync(originalFilePath, request.FilmRecord.Resolution, outputPath, cancellationToken);

        // Логирование успешного завершения конвертации файла
        logger.LogInformation("Successful conversion: {Path}", outputPath);

        // Проверяем существование предыдущей версии фильма в хранилище
        if (await filmStorage.IsExistsAsync(request.FilmRecord, cancellationToken))
        {
            // Логируем информацию о замене существующего файла
            logger.LogInformation("Replacing existing film version: {Path}", outputPath);
    
            // Удаляем предыдущую версию фильма из хранилища
            await filmStorage.DeleteAsync(request.FilmRecord, cancellationToken);
        }

        // Загружаем новую транскодированную версию фильма в хранилище
        await filmStorage.UploadAsync(request.FilmRecord, outputPath, cancellationToken);

        // Создание события о завершении обработки
        var @event = new VersionDownloadedIntegrationEvent
        {
            FilmId = request.FilmRecord.Id,
            Version = request.FilmRecord.Version,
            Season = request.FilmRecord.Season,
            Episode = request.FilmRecord.Episode
        };

        // Публикация события в шину сообщений
        await publishEndpoint.Publish(@event, cancellationToken);
    }
}