using Common.Application.FileStorage;
using Films.Application.Abstractions.Queries.Media;
using MediatR;

namespace Films.Application.Services.QueryHandlers.Media;

/// <summary>
/// Обработчик запроса на получение части файла фильма
/// </summary>
/// <param name="fileStore">Сервис для работы с файловым хранилищем</param>
public class GetFilmPartQueryHandler(IFileStorage fileStore) : IRequestHandler<GetFilmPartQuery, FileResult>
{
    /// <summary>
    /// Имя бакета в файловом хранилище, где хранятся файлы фильмов
    /// </summary>
    private const string FilmsBucket = "films";
    
    /// <summary>
    /// Обрабатывает запрос на получение части файла фильма
    /// </summary>
    /// <param name="request">Запрос на получение части файла</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат с потоком файла и информацией о content type</returns>
    /// <exception cref="ArgumentException">Выбрасывается при некорректных параметрах запроса</exception>
    /// <exception cref="FileNotFoundException">Выбрасывается если файл не найден в хранилище</exception>
    public async Task<FileResult> Handle(GetFilmPartQuery request, CancellationToken cancellationToken)
    {
        // Формируем ключ файла в хранилище на основе параметров запроса
        var key = BuildKey(request);

        // Получаем объект из S3 хранилища
        var (stream, contentType) = await fileStore.GetAsync(key, bucket: FilmsBucket, token: cancellationToken);

        // Возвращаем файл в виде результата
        return new FileResult(stream, contentType, request.FileName);
    }

    /// <summary>
    /// Генерирует ключ объекта в S3 на основе метаданных фильма
    /// </summary>
    /// <param name="request">Данные фильма</param>
    /// <returns>Ключ объекта в S3</returns>
    private static string BuildKey(GetFilmPartQuery request)
    {
        var parts = new List<string>
        {
            // ID фильма
            request.Id.ToString()
        };

        // Добавляем сезон, если указан
        if (request.Season.HasValue)
            parts.Add($"s{request.Season.Value:D2}");

        // Добавляем эпизод, если указан
        if (request.Episode.HasValue)
            parts.Add($"e{request.Episode.Value:D2}");

        // Версия
        parts.Add(request.Version);

        if (request.Resolution.HasValue)
            parts.Add(request.Resolution.Value.ToString());

        parts.Add(request.FileName);

        // Объединяем части через слеш
        return string.Join('/', parts);
    }
}