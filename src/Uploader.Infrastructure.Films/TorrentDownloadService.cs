using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
  /// Интервал между отчётами о проценте скачивания
  /// </summary>
  private static readonly TimeSpan ProgressReportInterval = TimeSpan.FromSeconds(10);

  /// <summary>
  /// Сколько ждать метаданные торрента, прежде чем считать, что пиров нет
  /// </summary>
  private static readonly TimeSpan MetadataTimeout = TimeSpan.FromMinutes(10);

  /// <summary>
  /// Имя файла кэша узлов DHT, из которого движок берёт начальные узлы при старте
  /// </summary>
  private const string DhtNodesCacheFile = "dht_nodes.cache";

  /// <summary>
  /// Узлы для входа в сеть DHT.
  /// </summary>
  /// <remarks>
  /// MonoTorrent знает только router.bittorrent.com и, если он недоступен (а из части сетей он не отвечает),
  /// DHT так и не запускается: magnet-ссылки без живых трекеров перестают находить пиров.
  /// </remarks>
  private static readonly (string Host, int Port)[] DhtBootstrapNodes =
  [
    ("dht.transmissionbt.com", 6881),
    ("dht.libtorrent.org", 25401),
    ("router.bittorrent.com", 6881),
    ("router.utorrent.com", 6881)
  ];

  /// <summary>
  /// Размер записи об узле DHT в компактном формате: идентификатор, IPv4-адрес и порт
  /// </summary>
  private const int CompactNodeLength = 20 + 4 + 2;

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

    string cacheDirectory = Path.Combine(savePath, ".engine");
    Directory.CreateDirectory(cacheDirectory);
    SeedDhtBootstrapNodes(Path.Combine(cacheDirectory, DhtNodesCacheFile));

    EngineSettings engineSettings = new EngineSettingsBuilder
    {
      MaximumConnections = 60,

      MaximumUploadRate = 1,

      CacheDirectory = cacheDirectory,

      AutoSaveLoadDhtCache = true
    }.ToSettings();

    _engine = new ClientEngine(engineSettings);
  }

  /// <summary>
  /// Основной метод загрузки фильма по magnet-ссылке
  /// </summary>
  /// <param name="uri">Magnet-ссылка для загрузки</param>
  /// <param name="filename">Имя файла (опционально)</param>
  /// <param name="onProgress">Обработчик изменения процента скачивания</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Путь к загруженному видеофайлу</returns>
  public async Task<string> DownloadAsync(string uri, string? filename, Func<double, Task>? onProgress,
    CancellationToken cancellationToken)
  {
    var magnet = MagnetLink.Parse(uri);
    TorrentManager? manager = _engine.Torrents.FirstOrDefault(t => t.InfoHashes == magnet.InfoHashes);

    if (manager == null)
    {
      try
      {
        ReadOnlyMemory<byte> metadata = await DownloadMetadataAsync(magnet, cancellationToken);
        _logger.LogInformation("Metadata downloaded. Loading torrent");
        var torrent = Torrent.Load(metadata.Span);
        manager = await _engine.AddAsync(torrent, _savePath);
      }
      catch (TorrentException e) when (e.Message == "A manager for this torrent has already been registered")
      {
        manager = _engine.Torrents.First(t => t.InfoHashes == magnet.InfoHashes);
      }
    }

    SemaphoreSlim semaphore = _managerLocks.GetOrAdd(manager, _ => new SemaphoreSlim(1, 1));
    _logger.LogInformation("Waiting for semaphore for torrent {Hash}", manager.InfoHashes.V1OrV2.ToHex());
    await semaphore.WaitAsync(cancellationToken);
    _logger.LogInformation("Semaphore acquired for torrent {Hash}", manager.InfoHashes.V1OrV2.ToHex());

    try
    {
      return await DownloadInternalAsync(manager, filename, onProgress, cancellationToken);
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
  /// <param name="onProgress">Обработчик изменения процента скачивания</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  private async Task<string> DownloadInternalAsync(TorrentManager manager, string? filename,
    Func<double, Task>? onProgress, CancellationToken cancellationToken)
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

    using var reportingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    Task reporting = onProgress == null
      ? Task.CompletedTask
      : ReportProgressAsync(file, onProgress, reportingCts.Token);

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
      await reportingCts.CancelAsync();
      await reporting;
      manager.TorrentStateChanged -= OnStateChanged;
      manager.PieceHashed -= OnPieceHashed;
    }
  }

  /// <summary>
  /// Периодически сообщает процент скачивания файла, пока не будет отменён
  /// </summary>
  /// <param name="file">Скачиваемый файл</param>
  /// <param name="onProgress">Обработчик изменения процента скачивания</param>
  /// <param name="cancellationToken">Токен остановки отчётов</param>
  private async Task ReportProgressAsync(ITorrentManagerFile file, Func<double, Task> onProgress,
    CancellationToken cancellationToken)
  {
    using var timer = new PeriodicTimer(ProgressReportInterval);
    double reported = -1;

    try
    {
      while (await timer.WaitForNextTickAsync(cancellationToken))
      {
        double percent = Math.Floor(file.BitField.PercentComplete);
        if (percent <= reported) continue;

        reported = percent;
        await onProgress(percent);
      }
    }
    catch (OperationCanceledException)
    {
    }
    catch (Exception e)
    {
      _logger.LogWarning(e, "Download progress reporting stopped for {FilePath}", file.Path);
    }
  }

  /// <summary>
  /// Скачивает метаданные торрента по magnet-ссылке с ограничением по времени
  /// </summary>
  /// <param name="magnet">Magnet-ссылка</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="TimeoutException">Если за отведённое время не удалось найти пиров</exception>
  private async Task<ReadOnlyMemory<byte>> DownloadMetadataAsync(MagnetLink magnet, CancellationToken cancellationToken)
  {
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeout.CancelAfter(MetadataTimeout);

    _logger.LogInformation("Downloading metadata for {Hash}. DHT: {State}, {Nodes} nodes",
      magnet.InfoHashes.V1OrV2.ToHex(), _engine.Dht.State, _engine.Dht.NodeCount);

    try
    {
      return await _engine.DownloadMetadataAsync(magnet, timeout.Token);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      throw new TimeoutException(
        $"Не удалось получить метаданные торрента за {MetadataTimeout.TotalMinutes:F0} мин: нет доступных пиров " +
        $"(трекеров в ссылке: {magnet.AnnounceUrls.Count}, DHT: {_engine.Dht.State}, узлов: {_engine.Dht.NodeCount})");
    }
  }

  /// <summary>
  /// Добавляет узлы для входа в сеть DHT в кэш узлов, если их там ещё нет.
  /// Узлы, накопленные движком в прошлых запусках, сохраняются.
  /// </summary>
  /// <param name="cacheFile">Путь к файлу кэша узлов DHT</param>
  private void SeedDhtBootstrapNodes(string cacheFile)
  {
    try
    {
      byte[] cached = File.Exists(cacheFile) ? File.ReadAllBytes(cacheFile) : [];
      if (cached.Length % CompactNodeLength != 0) cached = [];

      var knownEndpoints = new HashSet<string>();
      for (int offset = 0; offset < cached.Length; offset += CompactNodeLength)
        knownEndpoints.Add(Convert.ToHexString(cached, offset + 20, 6));

      using var nodes = new MemoryStream();
      nodes.Write(cached);

      foreach ((string host, int port) in DhtBootstrapNodes)
      {
        IPAddress[] addresses;
        try
        {
          addresses = Dns.GetHostAddresses(host);
        }
        catch (SocketException e)
        {
          _logger.LogWarning("Failed to resolve DHT bootstrap node {Host}: {Error}", host, e.Message);
          continue;
        }

        foreach (IPAddress address in addresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork))
        {
          byte[] endpoint = [.. address.GetAddressBytes(), (byte)(port >> 8), (byte)port];
          if (!knownEndpoints.Add(Convert.ToHexString(endpoint))) continue;

          // Идентификатор узла заранее неизвестен: DHT узнает настоящий из ответа на первый запрос
          nodes.Write(RandomNumberGenerator.GetBytes(20));
          nodes.Write(endpoint);
        }
      }

      File.WriteAllBytes(cacheFile, nodes.ToArray());
    }
    catch (Exception e) when (e is IOException or UnauthorizedAccessException)
    {
      _logger.LogWarning(e, "Failed to seed DHT bootstrap nodes");
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