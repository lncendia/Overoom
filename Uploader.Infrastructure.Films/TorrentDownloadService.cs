using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Uploader.Application.Abstractions.Services;
using MonoTorrent;
using MonoTorrent.Client;

namespace Uploader.Infrastructure.Films;

/// <summary>
/// Сервис для загрузки видеофильмов через BitTorrent по magnet-ссылке.
/// Управляет процессом загрузки, ожидает завершения и возвращает путь к видеофайлу.
/// </summary>
public class TorrentDownloadService : IFilmDownloadService, IDisposable
{
    /// <summary>
    /// Клиентский движок BitTorrent для управления загрузками
    /// </summary>
    private readonly ClientEngine _engine;

    /// <summary>
    /// Путь для сохранения загруженных файлов
    /// </summary>
    private readonly string _savePath;

    /// <summary>
    /// Список разрешенных видеоформатов
    /// </summary>
    private static readonly string[] AllowedExtensions = [".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv"];

    /// <summary>
    /// Логгер для сервиса загрузки торрентов
    /// </summary>
    private readonly ILogger<TorrentDownloadService> _logger;

    /// <summary>
    /// Семафоры на каждый TorrentManager для синхронизации доступа
    /// </summary>
    private readonly ConcurrentDictionary<TorrentManager, SemaphoreSlim> _managerLocks = new();

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    /// <param name="savePath">Директория для сохранения загруженных файлов</param>
    /// <param name="logger">Логгер</param>
    public TorrentDownloadService(string savePath, ILogger<TorrentDownloadService> logger)
    {
        // Сохраняем путь для загрузки в поле класса
        _savePath = savePath;

        // Инициализируем логгер
        _logger = logger;

        // Проверяем, что путь не содержит расширения файла
        if (Path.HasExtension(savePath))
        {
            // Выбрасываем исключение если путь содержит расширение файла
            throw new ArgumentException("Указанный путь содержит расширение файла и не может быть директорией");
        }

        // Если директория не существует - создаем ее
        if (!Directory.Exists(savePath))
        {
            // Создаем директорию рекурсивно
            Directory.CreateDirectory(savePath);
        }

        // Создаем настройки движка с параметрами по умолчанию
        var engineSettings = new EngineSettingsBuilder
        {
            // Максимальное количество соединений
            MaximumConnections = 60,

            // Максимальная скорость отдачи (1 КБ/с для экономии трафика)
            MaximumUploadRate = 1
        }.ToSettings();

        // Инициализируем BitTorrent клиент с указанными настройками
        _engine = new ClientEngine(engineSettings);
    }

    /// <summary>
    /// Основной метод загрузки фильма по magnet-ссылке
    /// </summary>
    /// <param name="uri">Magnet-ссылка для загрузки</param>
    /// <param name="filename">Имя файла (опционально)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Путь к загруженному видеофайлу</returns>
    public async Task<string> DownloadAsync(string uri, string? filename, CancellationToken cancellationToken)
    {
        // Парсим magnet-ссылку в объект MagnetLink
        var magnet = MagnetLink.Parse(uri);

        // Объявляем переменную для менеджера торрента
        TorrentManager manager;

        try
        {
            // Загружаем метаданные торрента по magnet-ссылке
            var metadata = await _engine.DownloadMetadataAsync(magnet, cancellationToken);

            // Логируем факт загрузки метаданных
            _logger.LogInformation("Metadata downloaded. Loading torrent");

            // Создаём объект Torrent из метаданных
            var torrent = Torrent.Load(metadata.Span);

            // Добавляем торрент в движок для загрузки
            manager = await _engine.AddAsync(torrent, _savePath);
        }
        catch (TorrentException e) when (e.Message == "A manager for this torrent has already been registered")
        {
            // Находим уже зарегистрированный менеджер по InfoHash
            manager = _engine.Torrents.First(t => t.InfoHashes == magnet.InfoHashes);
        }

        // Получаем семафор для конкретного торрент-менеджера (или создаем новый)
        var semaphore = _managerLocks.GetOrAdd(manager, _ => new SemaphoreSlim(1, 1));

        // Логируем ожидание семафора
        _logger.LogInformation("Waiting for semaphore for torrent {Hash}", manager.InfoHashes);
        
        // Ожидаем получения семафора
        await semaphore.WaitAsync(cancellationToken);
        
        // Логируем успешное получение семафора
        _logger.LogInformation("Semaphore acquired for torrent {Hash}", manager.InfoHashes);

        try
        {
            // Вызываем внутренний метод загрузки
            return await DownloadInternalAsync(manager, filename, cancellationToken);
        }
        finally
        {
            // Освобождаем семафор в блоке finally для гарантии освобождения
            semaphore.Release();
        }
    }

    /// <summary>
    /// Внутренний метод загрузки с конкретным менеджером торрента
    /// </summary>
    /// <param name="manager">Torrent менеджер</param>
    /// <param name="filename">Имя файла (опционально)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    private async Task<string> DownloadInternalAsync(TorrentManager manager, string? filename, CancellationToken cancellationToken)
    {
        // Переменная для хранения выбранного файла
        ITorrentManagerFile? file = null;

        // Если в торренте только один файл
        if (manager.Torrent!.Files.Count == 1)
        {
            // Берем первый файл
            file = manager.Files[0];

            // Устанавливаем высокий приоритет загрузки
            await manager.SetFilePriorityAsync(file, Priority.High);

            // Логируем информацию о файле
            _logger.LogInformation("Single file found: {FilePath}", file.Path);
        }

        // Если файл не указан и файлов несколько
        else if (string.IsNullOrEmpty(filename))
        {
            // Выбрасываем исключение - требуется указать конкретный файл
            throw new InvalidOperationException("A torrent contains more than one file");
        }

        // Если нужно выбрать конкретный файл
        else
        {
            // Перебираем все файлы в торренте
            foreach (var torrentFile in manager.Files)
            {
                // Если нашли нужный файл (сравниваем без учета регистра)
                if (torrentFile.Path.Equals(filename, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Сохраняем ссылку на файл
                    file = torrentFile;

                    // Устанавливаем высокий приоритет загрузки
                    await manager.SetFilePriorityAsync(torrentFile, Priority.High);

                    // Логируем информацию о выбранном файле
                    _logger.LogInformation("Selected file found: {FilePath}", file.Path);

                    // Не продолжаем поиск
                    continue;
                }

                // Для остальных файлов устанавливаем приоритет "не загружать"
                await manager.SetFilePriorityAsync(torrentFile, Priority.DoNotDownload);
            }

            // Если файл не найден - выбрасываем исключение
            if (file == null) throw new InvalidOperationException("The specified file was not found");
        }

        // Проверяем расширение файла на соответствие разрешенным
        if (!AllowedExtensions.Contains(Path.GetExtension(file.Path).ToLowerInvariant()))
        {
            // Выбрасываем исключение если файл не видео
            throw new InvalidOperationException("The file is not a video");
        }

        // Регистрируем обработчик отмены операции
        await using var registration = cancellationToken.Register(() =>
        {
            // Логируем факт отмены
            _logger.LogInformation("Download was cancelled. Stopping torrent.");

            // Останавливаем загрузку при отмене
            _ = manager.StopAsync();
        });

        // Создаем источник завершения задачи для асинхронного ожидания
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Обработчик изменения состояния торрента
        void OnStateChanged(object? _, TorrentStateChangedEventArgs e)
        {
            // Обрабатываем различные состояния торрента
            switch (e.NewState)
            {
                case TorrentState.Seeding:
                    // Логируем завершение загрузки
                    _logger.LogInformation("Download completed for: {FilePath}", file.Path);
                    
                    // Устанавливаем успешное завершение задачи
                    tcs.TrySetResult();
                    break;

                case TorrentState.Error:
                    // Устанавливаем исключение при ошибке
                    tcs.TrySetException(new Exception($"Download terminated with an error: {e.NewState}"));
                    break;

                case TorrentState.Stopped:
                    // Логируем остановку торрента
                    _logger.LogInformation("Torrent was stopped.");
                    
                    // Устанавливаем отмену задачи
                    tcs.TrySetCanceled(cancellationToken);
                    break;
            }
        }

        // Обработчик хэширования частей файла
        void OnPieceHashed(object? _, PieceHashedEventArgs e)
        {
            // Получаем прогресс загрузки файла в процентах
            var fileProgress = file.BitField.PercentComplete;
            
            // Логируем прогресс загрузки (debug уровень)
            _logger.LogDebug("Progress for {FilePath}: {Progress:F2}%", file.Path, fileProgress);
        }

        // Подписываемся на события менеджера торрента
        manager.TorrentStateChanged += OnStateChanged;
        manager.PieceHashed += OnPieceHashed;

        try
        {
            // Логируем начало загрузки
            _logger.LogInformation("Starting torrent");

            // Запускаем загрузку торрента
            await manager.StartAsync();

            // Логируем ожидание завершения загрузки
            _logger.LogInformation("Waiting for download to complete");

            // Ожидаем завершения задачи (успех, ошибка или отмена)
            await tcs.Task;

            // Останавливаем менеджер торрента после завершения
            await manager.StopAsync();

            // Проверяем существование загруженного файла на диске
            if (!File.Exists(file.FullPath))
            {
                // Логируем ошибку если файл не найден
                _logger.LogError("Downloaded file not found on disk: {FilePath}", file.FullPath);
                
                // Выбрасываем исключение
                throw new FileNotFoundException("Downloaded file not found on disk", file.FullPath);
            }

            // Возвращаем полный путь к загруженному файлу
            return file.FullPath;
        }
        finally
        {
            // Отписка от событий в любом случае (даже при исключении)
            manager.TorrentStateChanged -= OnStateChanged;
            manager.PieceHashed -= OnPieceHashed;
        }
    }

    /// <summary>
    /// Освобождение ресурсов
    /// </summary>
    public void Dispose()
    {
        // Подавляем финализацию
        GC.SuppressFinalize(this);

        // Освобождаем ресурсы движка
        _engine.Dispose();
        
        // Освобождаем все семафоры для каждого торрент-менеджера
        foreach (var semaphore in _managerLocks.Values)
        {
            // Освобождаем ресурсы семафора
            semaphore.Dispose();
        }
    }
}