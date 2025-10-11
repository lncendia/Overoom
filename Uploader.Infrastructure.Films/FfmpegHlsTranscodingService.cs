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
        // Проверка существования исходного файла
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("The source file was not found", inputPath);

        // Если выходной файл уже существует - удаляем его
        if (Directory.Exists(directory))
        {
            // Логируем предупреждение о том, что каталог будет очищен
            logger.LogWarning("The {outputDirectory} directory will be cleared", directory);
    
            // Рекурсивно удаляем каталог со всем содержимым
            Directory.Delete(directory, recursive: true);
        }

        // Получаем базовую директорию приложения
        var baseDir = AppContext.BaseDirectory;

        // Определяем платформо-специфичную папку
        var runtime = GetPlatformFolder();

        // Формируем путь к бинарнику FFmpeg
        var ffmpegPath = Path.Combine(baseDir, runtime, GetFfmpegFileName());

        // Проверяем наличие бинарника FFmpeg
        if (!File.Exists(ffmpegPath))
            throw new FileNotFoundException($"ffmpeg binary not found at {ffmpegPath}");

        // Получаем параметры масштабирования для FFmpeg
        var settingsList = GetHlsVariants(resolution);

        // Формируем аргументы командной строки для запуска FFmpeg
        var arguments = BuildHlsArgs(inputPath, directory, settingsList);

        // Настраиваем параметры запуска процесса
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

        // Запускаем процесс FFmpeg с указанными параметрами
        using var process = Process.Start(startInfo)!;

        logger.LogInformation("Transcoding started");
        
        // Подписываемся на события вывода стандартного потока (stdout)
        process.OutputDataReceived += (_, args) =>
        {
            // Проверяем, что данные не пустые
            if (!string.IsNullOrEmpty(args.Data))
            {
                // Логируем stdout вывод FFmpeg в транзитном режиме (информационное сообщение)
                logger.LogDebug("[FFmpeg stdout] {Output}", args.Data);
            }
        };

        // Подписываемся на события вывода ошибок (stderr)
        process.ErrorDataReceived += (_, args) =>
        {
            // Проверяем, что данные не пустые
            if (!string.IsNullOrEmpty(args.Data))
            {
                // Логируем stderr вывод FFmpeg (может содержать предупреждения и ошибки)
                logger.LogDebug("[FFmpeg stderr] {Error}", args.Data);
            }
        };

        // Начинаем асинхронное чтение стандартного потока вывода
        process.BeginOutputReadLine();

        // Начинаем асинхронное чтение потока ошибок
        process.BeginErrorReadLine();

        // Ожидаем завершения процесса
        await process.WaitForExitAsync(cancellationToken);

        // Проверяем код возврата
        if (process.ExitCode != 0)
        {
            // Читаем сообщение об ошибке
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"FFmpeg failed with an error ({process.ExitCode}): {error}");
        }
    }

    /// <summary>
    /// Предопределенные варианты качества HLS-стримов с параметрами кодирования
    /// Содержит настройки для различных разрешений от 360p до 4K
    /// </summary>
    private static readonly IReadOnlyList<HlsVariant> Variants = new List<HlsVariant>
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
        return Variants.Where(v => v.Resolution <= source).ToList();
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
        // Определяем количество вариантов качества для разделения видео
        var splitCount = variants.Count;

        // Создаем фильтры масштабирования для каждого варианта качества
        var scaleFilters = string.Join(" ", variants.Select((v, i) =>
            $"[v{i + 1}]scale=w={v.Size.Split('x')[0]}:h={v.Size.Split('x')[1]}[v{i + 1}out];"
        ));

        // Создаем комплексный фильтр для разделения видео на несколько потоков
        var filterComplex = $"[0:v]split={splitCount}" +
                            string.Concat(Enumerable.Range(1, splitCount).Select(i => $"[v{i}]")) +
                            $"; {scaleFilters}".TrimEnd(';');

        // Строим карты потоков (map) для видео и аудио
        var maps = new StringBuilder();
        // Строим карту переменных потоков для HLS манифеста
        var varStreamMap = new StringBuilder();

        // Обрабатываем каждый вариант качества
        for (var i = 0; i < variants.Count; i++)
        {
            var name = variants[i].Resolution.ToString();

            // Добавляем параметры кодирования видео для текущего варианта
            maps.Append($"""
                             -map "[v{i + 1}out]" -c:v:{i} h264_nvenc -preset fast -b:v:{i} {variants[i].Bitrate} -maxrate:v:{i} {variants[i].Maxrate} -bufsize:v:{i} {variants[i].Bufsize} 
                             -map a:0 -c:a:{i} aac -b:a:{i} {variants[i].AudioBitrate} -ac 2 
                         """);

            // Добавляем информацию о потоке в карту переменных
            varStreamMap.Append($"v:{i},a:{i},name:{name} ");
        }

        // Формируем итоговую строку аргументов FFmpeg
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
                """.Trim().Replace(Environment.NewLine, " ");
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
    private static string GetFfmpegFileName() => OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
}