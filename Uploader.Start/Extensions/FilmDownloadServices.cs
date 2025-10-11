using Uploader.Infrastructure.Films;
using Common.DI.Extensions;
using Uploader.Application.Abstractions.Services;

namespace Uploader.Start.Extensions;

/// <summary>
/// Статический класс расширений для регистрации сервисов загрузки фильмов
/// </summary>
public static class FilmDownloadServices
{
    /// <summary>
    /// Регистрирует все сервисы загрузки фильмов в DI-контейнере
    /// </summary>
    /// <param name="builder">Построитель веб-приложения.</param>
    public static void AddFilmDownloadServices(this IHostApplicationBuilder builder)
    {
        // Получение пути сохранения файлов Torrent;
        var path = builder.Configuration.GetRequiredValue<string>("Torrent:Path");

        // Добавление сервиса загрузки фильмов.
        builder.Services.AddSingleton<IFilmDownloadService>(sp =>
            new TorrentDownloadService(path, sp.GetRequiredService<ILogger<TorrentDownloadService>>()));
    }
}