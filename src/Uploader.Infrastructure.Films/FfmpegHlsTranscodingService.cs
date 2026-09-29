using System.Diagnostics;
using System.Text;
using Common.Domain.Enums;
using Uploader.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Uploader.Infrastructure.Films;

/// <summary>
/// Сервис для транскодирования видеофайлов с помощью FFmpeg
/// </summary>
public class FfmpegHlsTranscodingService(ILogger<FfmpegHlsTranscodingService> logger) : IHlsTranscodingService
{
  /// <summary>
  /// Основной метод транскодирования видеофайла
  /// </summary>
  /// <param name="inputPath">Путь к исходному файлу</param>
  /// <param name="resolution">Целевое разрешение видео</param>
  /// <param name="directory">Путь для сохранения HLS данных</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="FileNotFoundException">Если исходный файл не найден</exception>
  /// <exception cref="Exception">При ошибке выполнения FFmpeg</exception>
  public async Task TranscodeAsync(string inputPath, FilmResolution resolution, string directory,
    CancellationToken cancellationToken)
  {
    if (!File.Exists(inputPath))
      throw new FileNotFoundException("The source file was not found", inputPath);

    if (Directory.Exists(directory))
    {
      logger.LogWarning("The {outputDirectory} directory will be cleared", directory);
      Directory.Delete(directory, recursive: true);
    }

    string baseDir = AppContext.BaseDirectory;
    string runtime = GetPlatformFolder();
    string ffmpegPath = Path.Combine(baseDir, runtime, GetFfmpegFileName());

    if (!File.Exists(ffmpegPath))
      throw new FileNotFoundException($"ffmpeg binary not found at {ffmpegPath}");

    List<HlsVariant> settingsList = GetHlsVariants(resolution);
    string arguments = BuildHlsArgs(inputPath, directory, settingsList);

    var startInfo = new ProcessStartInfo
    {
      FileName = ffmpegPath,
      Arguments = arguments,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true,
      StandardOutputEncoding = Encoding.UTF8,
      StandardErrorEncoding = Encoding.UTF8
    };

    using var process = Process.Start(startInfo)!;

    logger.LogInformation("Transcoding started");

    process.OutputDataReceived += (_, args) =>
    {
      if (!string.IsNullOrEmpty(args.Data))
      {
        logger.LogDebug("[FFmpeg stdout] {Output}", args.Data);
      }
    };

    process.ErrorDataReceived += (_, args) =>
    {
      if (!string.IsNullOrEmpty(args.Data))
      {
        logger.LogDebug("[FFmpeg stderr] {Error}", args.Data);
      }
    };

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    try
    {
      await process.WaitForExitAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
      logger.LogInformation("Transcoding was cancelled. Stopping FFmpeg");
      process.Kill(entireProcessTree: true);
      throw;
    }

    if (process.ExitCode != 0)
    {
      string error = await process.StandardError.ReadToEndAsync(cancellationToken);
      throw new Exception($"FFmpeg failed with an error ({process.ExitCode}): {error}");
    }
  }

  /// <summary>
  /// Предопределенные варианты качества HLS-стримов с параметрами кодирования
  /// Содержит настройки для различных разрешений от 360p до 4K
  /// </summary>
  private static readonly IReadOnlyList<HlsVariant> _variants = new List<HlsVariant>
  {
    new(FilmResolution.P2160, "3840x2160", "12000k", "12840k", "19200k", "384k"),
    new(FilmResolution.P1080, "1920x1080", "5000k", "5350k", "7500k", "192k"),
    new(FilmResolution.P720, "1280x720", "2800k", "2996k", "4200k", "128k"),
    new(FilmResolution.P480, "854x480", "1400k", "1498k", "2100k", "96k"),
    new(FilmResolution.P360, "640x360", "800k", "856k", "1200k", "64k")
  };

  /// <summary>
  /// Возвращает список вариантов качества HLS подходящих для исходного разрешения
  /// Фильтрует варианты, оставляя только те, что меньше или равны исходному разрешению
  /// </summary>
  /// <param name="source">Исходное разрешение видеофайла</param>
  /// <returns>Отфильтрованный список вариантов качества для транскодирования</returns>
  private static List<HlsVariant> GetHlsVariants(FilmResolution source)
  {
    return [.. _variants.Where(v => v.Resolution <= source)];
  }

  /// <summary>
  /// Параметры варианта качества HLS-стрима
  /// Определяет настройки кодирования для конкретного разрешения видео
  /// </summary>
  /// <param name="Resolution">Разрешение видео (например, 1080p, 720p, 480p)</param>
  /// <param name="Size">Размер кадра в формате "ширинаxвысота" (например, "1920x1080")</param>
  /// <param name="Bitrate">Битрейт видео в битах в секунду (например, "4000k")</param>
  /// <param name="Maxrate">Максимальный битрейт видео для VBR кодирования (например, "5000k")</param>
  /// <param name="Bufsize">Размер буфера кодирования (например, "8000k")</param>
  /// <param name="AudioBitrate">Битрейт аудио дорожки (например, "128k")</param>
  private record HlsVariant(
    FilmResolution Resolution,
    string Size,
    string Bitrate,
    string Maxrate,
    string Bufsize,
    string AudioBitrate);

  /// <summary>
  /// Строит аргументы командной строки для FFmpeg для генерации HLS-потоков
  /// с несколькими вариантами качества (вариативными стримами)
  /// </summary>
  /// <param name="input">Путь к исходному видеофайлу</param>
  /// <param name="outputDir">Директория для выходных HLS-файлов</param>
  /// <param name="variants">Список вариантов качества с параметрами кодирования</param>
  /// <returns>Строка аргументов для запуска FFmpeg</returns>
  private static string BuildHlsArgs(string input, string outputDir, List<HlsVariant> variants)
  {
    int splitCount = variants.Count;

    string scaleFilters = string.Join(" ", variants.Select((v, i) =>
      $"[v{i + 1}]scale=w={v.Size.Split('x')[0]}:h={v.Size.Split('x')[1]}[v{i + 1}out];"
    ));

    string filterComplex = $"[0:v]split={splitCount}" +
                           string.Concat(Enumerable.Range(1, splitCount).Select(i => $"[v{i}]")) +
                           $"; {scaleFilters}".TrimEnd(';');

    var maps = new StringBuilder();
    var varStreamMap = new StringBuilder();

    for (int i = 0; i < variants.Count; i++)
    {
      string name = variants[i].Resolution.ToString();

      maps.Append($"""
                       -map "[v{i + 1}out]" -c:v:{i} h264_nvenc -preset fast -b:v:{i} {variants[i].Bitrate} -maxrate:v:{i} {variants[i].Maxrate} -bufsize:v:{i} {variants[i].Bufsize} 
                       -map a:0 -c:a:{i} aac -b:a:{i} {variants[i].AudioBitrate} -ac 2 
                   """);

      varStreamMap.Append($"v:{i},a:{i},name:{name} ");
    }

    return $"""
                -i "{input}" 
                -filter_complex "{filterComplex}" 
                {maps}
                -f hls 
                -hls_time 10 
                -hls_playlist_type vod 
                -hls_flags independent_segments 
                -hls_segment_type mpegts 
                -hls_segment_filename "{outputDir}/%v/data%03d.ts" 
                -master_pl_name master.m3u8 
                -var_stream_map "{varStreamMap.ToString().Trim()}" 
                "{outputDir}/%v/index.m3u8"
            """.Trim().ReplaceLineEndings(" ");
  }

  /// <summary>
  /// Определяет платформо-специфичную папку для бинарников FFmpeg
  /// </summary>
  /// <returns>Имя папки с бинарниками</returns>
  /// <exception cref="PlatformNotSupportedException">Для неподдерживаемых платформ</exception>
  private static string GetPlatformFolder()
  {
    if (OperatingSystem.IsWindows()) return "win-x64";
    if (OperatingSystem.IsLinux()) return "linux-x64";
    if (OperatingSystem.IsMacOS()) return "osx-x64";
    throw new PlatformNotSupportedException();
  }

  /// <summary>
  /// Возвращает имя исполняемого файла FFmpeg в зависимости от ОС
  /// </summary>
  /// <returns>"ffmpeg.exe" для Windows, "ffmpeg" для других ОС</returns>
  private static string GetFfmpegFileName()
  {
    return OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
  }
}