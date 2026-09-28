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
  private static readonly string[] _allowedExtensions = [".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv"];

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
    _savePath = savePath;
    _logger = logger;

    if (Path.HasExtension(savePath))
    {
      throw new ArgumentException("Указанный путь содержит расширение файла и не может быть директорией");
    }

    if (!Directory.Exists(savePath))
    {
      Directory.CreateDirectory(savePath);
    }

    EngineSettings engineSettings = new EngineSettingsBuilder
    {
      MaximumConnections = 60,

      MaximumUploadRate = 1
    }.ToSettings();

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
    var magnet = MagnetLink.Parse(uri);
    TorrentManager manager;

    try
    {
      ReadOnlyMemory<byte> metadata = await _engine.DownloadMetadataAsync(magnet, cancellationToken);
      _logger.LogInformation("Metadata downloaded. Loading torrent");
      var torrent = Torrent.Load(metadata.Span);
      manager = await _engine.AddAsync(torrent, _savePath);
    }
    catch (TorrentException e) when (e.Message == "A manager for this torrent has already been registered")
    {
      manager = _engine.Torrents.First(t => t.InfoHashes == magnet.InfoHashes);
    }

    SemaphoreSlim semaphore = _managerLocks.GetOrAdd(manager, _ => new SemaphoreSlim(1, 1));
    _logger.LogInformation("Waiting for semaphore for torrent {Hash}", manager.InfoHashes);
    await semaphore.WaitAsync(cancellationToken);
    _logger.LogInformation("Semaphore acquired for torrent {Hash}", manager.InfoHashes);

    try
    {
      return await DownloadInternalAsync(manager, filename, cancellationToken);
    }
    finally
    {
      semaphore.Release();
    }
  }

  /// <summary>
  /// Внутренний метод загрузки с конкретным менеджером торрента
  /// </summary>
  /// <param name="manager">Torrent менеджер</param>
  /// <param name="filename">Имя файла (опционально)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  private async Task<string> DownloadInternalAsync(TorrentManager manager, string? filename,
    CancellationToken cancellationToken)
  {
    ITorrentManagerFile? file = null;

    if (manager.Torrent!.Files.Count == 1)
    {
      file = manager.Files[0];
      await manager.SetFilePriorityAsync(file, Priority.High);
      _logger.LogInformation("Single file found: {FilePath}", file.Path);
    }

    else if (string.IsNullOrEmpty(filename))
    {
      throw new InvalidOperationException("A torrent contains more than one file");
    }

    else
    {
      foreach (ITorrentManagerFile torrentFile in manager.Files)
      {
        if (torrentFile.Path.Equals(filename, StringComparison.CurrentCultureIgnoreCase))
        {
          file = torrentFile;
          await manager.SetFilePriorityAsync(torrentFile, Priority.High);
          _logger.LogInformation("Selected file found: {FilePath}", file.Path);
          continue;
        }

        await manager.SetFilePriorityAsync(torrentFile, Priority.DoNotDownload);
      }

      if (file == null) throw new InvalidOperationException("The specified file was not found");
    }

    if (!_allowedExtensions.Contains(Path.GetExtension(file.Path).ToLowerInvariant()))
    {
      throw new InvalidOperationException("The file is not a video");
    }

    await using CancellationTokenRegistration registration = cancellationToken.Register(() =>
    {
      _logger.LogInformation("Download was cancelled. Stopping torrent.");
      _ = manager.StopAsync();
    });

    var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    void OnStateChanged(object? _, TorrentStateChangedEventArgs e)
    {
      switch (e.NewState)
      {
        case TorrentState.Seeding:
          _logger.LogInformation("Download completed for: {FilePath}", file.Path);

          tcs.TrySetResult();
          break;

        case TorrentState.Error:
          tcs.TrySetException(new Exception($"Download terminated with an error: {e.NewState}"));
          break;

        case TorrentState.Stopped:
          _logger.LogInformation("Torrent was stopped.");

          tcs.TrySetCanceled(cancellationToken);
          break;
      }
    }

    void OnPieceHashed(object? _, PieceHashedEventArgs e)
    {
      double fileProgress = file.BitField.PercentComplete;
      _logger.LogDebug("Progress for {FilePath}: {Progress:F2}%", file.Path, fileProgress);
    }

    manager.TorrentStateChanged += OnStateChanged;
    manager.PieceHashed += OnPieceHashed;

    try
    {
      _logger.LogInformation("Starting torrent");
      await manager.StartAsync();
      _logger.LogInformation("Waiting for download to complete");
      await tcs.Task;
      await manager.StopAsync();

      if (!File.Exists(file.FullPath))
      {
        _logger.LogError("Downloaded file not found on disk: {FilePath}", file.FullPath);
        throw new FileNotFoundException("Downloaded file not found on disk", file.FullPath);
      }

      return file.FullPath;
    }
    finally
    {
      manager.TorrentStateChanged -= OnStateChanged;
      manager.PieceHashed -= OnPieceHashed;
    }
  }

  /// <summary>
  /// Освобождение ресурсов
  /// </summary>
  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _engine.Dispose();

    foreach (SemaphoreSlim semaphore in _managerLocks.Values)
    {
      semaphore.Dispose();
    }
  }
}